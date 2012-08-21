/*
	AsyncMethodTransform.cs: Cirrus Coroutine Transformation
   
	Copyright (c) 2010 - 2011 Alexander Corrado
  
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
 */

using System;
using System.Linq;
using System.Reflection;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecil.Decompiler.Cil;

using Cirrus.Tools.Cilc.Util;

namespace Cirrus.Tools.Cilc.Targets.IL {
	
	// FIXME: Finish factoring/ Clean this up a little bit
	public class AsyncMethodTransform : BaseInstructionTransform {
		
		private static uint asyncMethodID = 0;

		TypeDefinition future, coroutineFuture, coroutineFutureT;
		MethodReference isScheduled, schedule, getException, setException, setStatus, setValue, getValue;
		
		protected AssemblyDefinition core;
		protected ModuleDefinition module;
		protected MethodDefinition method;
		
		protected TypeDefinition coroutineType;
		protected MethodDefinition coroutine;
		protected List<Instruction> continuations;
		protected FieldReference [] argsFields, localsFields;
		
		protected bool debug;
		
		protected GenericInstanceType genType;
		
		// These references will vary depending on whether
		//  the coroutine inherits from CoroutineFuture or CoroutineFuture<>	
		MethodReference baseCtor;
		MethodReference chain;
		MethodReference checkException;
		
		FieldReference threadFld; // reference to thread to reschedule on
		FieldReference chainedFld; // holds the chained future to pull value/exception upon continuation
		FieldReference pcFld; // program counter (continuation selector)
		FieldReference epcFld; // catch/finally block selector (bitfield)
		
		VariableDefinition dupLoc; // local for isolating a stack item from a Dup
		Dictionary<IGenericParameterProvider,int> skews;
		
		// the return statement that all leave ops will point to
		Instruction ret = Instruction.Create (OpCodes.Ret);
		
		// FYI, this indicates all the places where continuations will be inserted (and must be updated accordingly :)
		private static bool IsContinuation (Instruction instruction)
		{
			if (instruction.OpCode.FlowControl != FlowControl.Call)
					return false;
				
			var callee = instruction.Operand as MethodReference;
			
			return (callee.Name == "Wait" && callee.DeclaringType.IsFutureType ()) ||
			       (callee.Name == "Yield" && callee.DeclaringType.FullName == "Cirrus.Thread");
		}
		
		static AsyncMethodTransform ()
		{
			IsBarrier = IsContinuation;
		}
		
		public static void Transform (MethodDefinition method, AssemblyDefinition core, bool debuggable)
		{
			(new AsyncMethodTransform (method, core, debuggable)).Execute ();
		}
		
		private AsyncMethodTransform (MethodDefinition method, AssemblyDefinition core, bool debuggable)
		{
			this.method = method;
			this.module = method.Module;
			this.core  = core;
			this.debug = debuggable;
			VerifyTarget ();

			this.argsFields = new FieldReference [method.Parameters.Count + (method.HasThis? 1 : 0)];
			this.localsFields = new FieldReference [method.Body.Variables.Count];
			
			this.skews = new Dictionary<IGenericParameterProvider,int> (2);
			skews.Add (method.DeclaringType, 0);
			skews.Add (method, method.DeclaringType.GenericParameters.Count);
			
			this.continuations = new List<Instruction> ();
		}
		
		protected virtual void VerifyTarget ()
		{
			if (!module.Assembly.CustomAttributes.Any (a => 
				a.AttributeType.FullName == "System.Runtime.CompilerServices.RuntimeCompatibilityAttribute" &&
				a.Properties.Any (f => f.Name == "WrapNonExceptionThrows" && (bool)f.Argument.Value))
			   )
			   throw new NotSupportedException ("Assemblies without RuntimeCompatibilityAttribute(WrapNonExceptionThrows = true)");	
		}
		
		public virtual void Execute ()
		{
			LoadReferences ();
			CreateCoroutineType ();
			coroutineType.BaseType = ImportBaseTypeReferences (coroutineType);
			
			method.DeclaringType.NestedTypes.Add (coroutineType);
			
			var ctor = CreateCoroutineConstructor ();
			coroutineType.Methods.Add (ctor);
			
			CreateCoroutine ();
			coroutineType.Methods.Add (coroutine);
			
			// replace original method with a call to the coroutine
			RewriteOriginalMethodBody (coroutineType, ctor);
			
			// finish constructor
			var il = ctor.Body.GetILProcessor ();
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Call, genType != null ? coroutine.MakeGeneric (genType) : coroutine);

			il.Emit (OpCodes.Ret);
			ctor.Body.ComputeOffsetsAndMaxStack ();
		}
		
		protected virtual void CreateCoroutineType ()
		{
			var typeGeneric = new GenericParameter [method.DeclaringType.GenericParameters.Count];
			var methodGeneric = new GenericParameter [method.GenericParameters.Count];
			
			int i = typeGeneric.Length + methodGeneric.Length;
			
			var futureName = string.Format ("__c{0}_{1}_{2}_impl", asyncMethodID++, method.Name.Replace ("`", "$"),
			                                string.Join ("_", method.Parameters.Select (p => p.ParameterType.Name.Replace ("`", "$")).ToArray ()));
			if (i > 0)
				futureName += string.Format ("`{0}", i);
			
			coroutineType = new TypeDefinition (null, futureName, Mono.Cecil.TypeAttributes.NestedPrivate | Mono.Cecil.TypeAttributes.Sealed | Mono.Cecil.TypeAttributes.BeforeFieldInit);
			
			//if (debug)
			//	coroutineType.CustomAttributes.Add (new CustomAttribute (module.Import (compilerGenerated)));
			
			// copy all generic parameters from method's containing type and method into new coroutinefuture type
			for (i = 0; i < typeGeneric.Length; i++) {
				typeGeneric [i] = new GenericParameter (method.DeclaringType.GenericParameters [i].Name, coroutineType);
				foreach (var constraint in method.DeclaringType.GenericParameters [i].Constraints)
					typeGeneric [i].Constraints.Add (constraint);
				foreach (var attr in method.DeclaringType.GenericParameters [i].CustomAttributes)
					typeGeneric [i].CustomAttributes.Add (attr);
				coroutineType.GenericParameters.Add (typeGeneric [i]);
			}
			for (i = 0; i < methodGeneric.Length; i++) {
				methodGeneric [i] = new GenericParameter (method.GenericParameters [i].Name, coroutineType);
				foreach (var constraint in method.GenericParameters [i].Constraints)
					methodGeneric [i].Constraints.Add (constraint);
				foreach (var attr in method.GenericParameters [i].CustomAttributes)
					methodGeneric [i].CustomAttributes.Add (attr);
				coroutineType.GenericParameters.Add (methodGeneric [i]);
			}

			if (typeGeneric.Length != 0 || methodGeneric.Length != 0) {
				genType = new GenericInstanceType (coroutineType);
				foreach (var genParam in coroutineType.GenericParameters)
					genType.GenericArguments.Add (genParam);
			}
			
			// add fields for original method's "this" and other arguments..
			i = 0;
			string name;
			FieldDefinition fld;
			
			if (method.HasThis) {
				
				var thisType = method.DeclaringType.MakeGeneric (typeGeneric);
				argsFields [0] = fld = new FieldDefinition ("$ths", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.InitOnly, thisType);
				coroutineType.Fields.Add (fld);
				if (genType != null)
					argsFields [0] = new FieldReference (fld.Name, fld.FieldType, genType);
				i = 1;
				
			}
			foreach (var arg in method.Parameters) {
				var paramType = arg.ParameterType.CopyGeneric (coroutineType, skews);
				
				name = "$a" + i;
				if (debug)
					name = arg.Name ?? name;
				argsFields [i] = fld = new FieldDefinition (name, Mono.Cecil.FieldAttributes.Private, paramType);
				coroutineType.Fields.Add (fld);
				if (genType != null)
					argsFields [i] = new FieldReference (fld.Name, fld.FieldType, genType);
				i++;
			}
			
			// create a field for each local
			i = 0;
			foreach (var local in method.Body.Variables) {
				name = "$l" + i;
				if (debug)
					name = local.Name ?? name;
				localsFields [i] = fld = new FieldDefinition (name, Mono.Cecil.FieldAttributes.Private, local.VariableType.CopyGeneric (coroutineType, skews));
				coroutineType.Fields.Add (fld);
				if (genType != null)
					localsFields [i] = new FieldReference (fld.Name, fld.FieldType, genType);
				i++;
			}
		}
		
		protected virtual void LoadReferences ()
		{
			if (future != null)
				return;

			var i = 0;

			future = core.MainModule.Types.SingleOrDefault (t => t.FullName == typeof (Cirrus.Future).FullName);
			var futureT = core.MainModule.Types.SingleOrDefault (t => t.FullName == typeof (Cirrus.Future<>).FullName);
			coroutineFuture = core.MainModule.Types.SingleOrDefault (t => t.FullName == typeof (Cirrus.CoroutineFuture).FullName);
			coroutineFutureT = core.MainModule.Types.SingleOrDefault (t => t.FullName == typeof (Cirrus.CoroutineFuture<>).FullName);

			if (future == null)
				throw new Exception ("Could not load Cirrus.Future type from provided Cirrus.Core assembly");
			
			foreach (var method in future.Methods) {
				switch (method.Name) {
				case "get_IsScheduled": isScheduled = method; i++; break;
				case "Schedule": if (method.HasParameters) { schedule = method; i++; } break;
				case "get_Exception": getException = method; i++; break;
				case "set_Exception": setException = method; i++; break;
				case "set_Status": setStatus = method; i++; break;
				}
				if (i >= 5)
					break;
			}
			if (i < 5)
				throw new Exception ("Could not load all the references need from the Cirrus.Core assembly provided");
			
			i = 0;
			foreach (var method in futureT.Methods) {
				switch (method.Name) {
				case "set_Value": setValue = method; i++; break;
				case "get_Value": getValue = method; i++; break;
				}
				if (i >= 2)
					break;
			}
			if (i < 2)
				throw new Exception ("Could not load all the references need from the Cirrus.Core assembly provided");
		}
			
		protected virtual TypeReference ImportBaseTypeReferences (TypeDefinition coroutineType)
		{
			TypeReference baseType = null;
			var returnType = method.ReturnType.CopyGeneric (coroutineType, skews);
			
			if (returnType.IsGenericInstance) { // async method returns Future<T>
				
				var futureValueType = ((GenericInstanceType)returnType).GenericArguments [0].CopyGeneric (coroutineType, skews);
				baseType = module.Import (coroutineFutureT).MakeGeneric (futureValueType);
				
				baseCtor = module.ImportFrom (coroutineFutureT.Methods.First (m => m.IsConstructor), baseType);
				threadFld = module.ImportFrom (coroutineFutureT.Fields.First (f => f.Name == "thread"), baseType);
				chainedFld = module.ImportFrom (coroutineFutureT.Fields.First (f => f.Name == "chained"), baseType);
				pcFld = module.ImportFrom (coroutineFutureT.Fields.First (f => f.Name == "pc"), baseType);
				epcFld = module.ImportFrom (coroutineFutureT.Fields.First (f => f.Name == "epc"), baseType);
				chain = module.ImportFrom (coroutineFutureT.Methods.First (m => m.Name == "Chain"), baseType);
				checkException = module.ImportFrom (coroutineFutureT.Methods.First (m => m.Name == "CheckException"), baseType);

			} else { // returns Future or void...
				
				baseType = module.Import (coroutineFuture);

				baseCtor = module.ImportFrom (coroutineFuture.Methods.First (m => m.IsConstructor), baseType);
				threadFld = module.ImportFrom (coroutineFuture.Fields.First (f => f.Name == "thread"), baseType);
				chainedFld = module.ImportFrom (coroutineFuture.Fields.First (f => f.Name == "chained"), baseType);
				pcFld = module.ImportFrom (coroutineFuture.Fields.First (f => f.Name == "pc"), baseType);
				epcFld = module.ImportFrom (coroutineFuture.Fields.First (f => f.Name == "epc"), baseType);
				chain = module.ImportFrom (coroutineFuture.Methods.First (m => m.Name == "Chain"), baseType);
				checkException = module.ImportFrom (coroutineFuture.Methods.First (m => m.Name == "CheckException"), baseType);
				
			}
			
			return baseType;
		}
		
		protected virtual MethodDefinition CreateCoroutineConstructor ()
		{
			// create ctor
			var ctor = new MethodDefinition (".ctor", Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName |
			                                   Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Public, module.TypeSystem.Void);

			// hide the ctor from the debugger if we're in debug mode
			//if (debug)
			//	ctor.CustomAttributes.Add (new CustomAttribute (module.Import (debuggerHidden)));
			
			var il = ctor.Body.GetILProcessor ();
			
			// first, call base ctor
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Call, baseCtor);
			
			// load all args
			for (var i = 0; i < argsFields.Length; i++) {
				ctor.Parameters.Add (new ParameterDefinition (argsFields [i].FieldType));
				
				// this.$argX = <ArgX>
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg, i + 1);
				il.Emit (OpCodes.Stfld, argsFields [i]);
			}
			
			return ctor;
		}
		
		// FIXME: Clean up.. factor out functionality into smaller, better pieces? Remove some generated nops?
		protected virtual void CreateCoroutine ()
		{
			// create coroutine method
			coroutine = new MethodDefinition ("Resume", Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Public |
			                                      Mono.Cecil.MethodAttributes.Virtual, module.TypeSystem.Void);
			
			var il = coroutine.Body.GetILProcessor ();
			
			// process the method we want to transform and...
			//  1) Replace all Ldarg, Starg opcodes with Ldfld, Stfld (argsFields)
			//  2) Replace all Ldloc, Stloc opcodes with Ldfld, Stfld (localsFields)
			//  3) Before Ret, add Future<T>.Value = ...; or Future.Status = FutureStatus.Fulfilled;
			//  4) Replace calls to Future.Wait or Thread.Yield with continuation
			//  5) Remove calls to Future<T>.op_Implicit preceeding Ret
			Visit (method);
			
			// remap the jumps- pass 1 (required because exception handling extracts some of these)
			MapAllInstructions (method.Body);
			
			// the whole body of the method must be an exception handler
			var exceptionType = module.Import (getException.ReturnType);
			var exception = new VariableDefinition (exceptionType);
			coroutine.Body.Variables.Add (exception);
			
			VariableDefinition skipFinallyBlocks = null;
			
			// add exception handlers
			int i = 0;
			var catchBlocks = new List<IList<Instruction>> ();
			var finallyBlocks = new List<IList<Instruction>> ();
			MethodDefinition finallyMethod = null;
			foreach (var eh in method.Body.ExceptionHandlers.OrderBy (e => e.TryStart.Offset).OrderBy (e => e.TryEnd.Offset)) {
				
				// set the next ePC to this handler block				
				// we can get away with just setting an exception block selector (epc)
				//  at the beginning of the block because jumps into the middle of a try block are not allowed
				//  (see ECMA-335 12.4.2.8.2.7)
				InsertBefore (eh.TryStart, new Instruction [] {
					il.Create (OpCodes.Ldarg_0),
					il.Create (OpCodes.Ldarg_0),
					il.Create (OpCodes.Ldfld, epcFld),
					il.Create (OpCodes.Ldc_I8, 1L << i),
					il.Create (OpCodes.Or),
					il.Create (OpCodes.Stfld, epcFld)
				});
				
				// clear the bit for this handler at end of try block
				// This awkward bit of code below is necessary because one handler's end can be another's start
				var clearEpc = new List<Instruction> () {
					il.Create (OpCodes.Ldarg_0),
					il.Create (OpCodes.Ldarg_0),
					il.Create (OpCodes.Ldfld, epcFld),
					il.Create (OpCodes.Ldc_I8, ~(1L << i)),
					il.Create (OpCodes.And),
					il.Create (OpCodes.Stfld, epcFld)
				};
				
				if (eh.HandlerType == ExceptionHandlerType.Finally) {
					finallyMethod = new MethodDefinition ("$finally" + i, Mono.Cecil.MethodAttributes.Private, module.TypeSystem.Void);
					coroutineType.Methods.Add (finallyMethod);
					clearEpc.Add (il.Create (OpCodes.Ldarg_0));
					clearEpc.Add (il.Create (OpCodes.Call, finallyMethod));
				}
				InstructionMap.Insert (InstructionMap.IndexOfKey (eh.HandlerEnd), clearEpc [0], clearEpc);
				
				// have to fixup leaves from catch blocks to unset our epcs for handlers that are all in the same spot
				var lastHandler = method.Body.ExceptionHandlers.OrderBy (h => h.HandlerStart.Offset).LastOrDefault ((l, c) => l.HandlerEnd == c.HandlerStart, eh);
				var subsequent = lastHandler.HandlerEnd;//InstructionMap [lastHandler.HandlerEnd].FirstOrDefault ();
				//if (subsequent != null)
				MapInstructions (subsequent, clearEpc [0]);

				// extract the handler block
				var handlerBody = ExtractInstructionRange (eh.HandlerStart, clearEpc [0]);
				var block = new List<Instruction> ();
				
				//fixup common issue
				// FIXME: this solution is a kludge
				if (handlerBody [0].OpCode == OpCodes.Stfld) {
					handlerBody.Insert (0, il.Create (OpCodes.Stloc, exception));
					handlerBody.Insert (1, il.Create (OpCodes.Ldarg_0));
					handlerBody.Insert (2, il.Create (OpCodes.Ldloc, exception));
					if (eh.CatchType.FullName != "System.Exception")
						handlerBody.Insert (3, il.Create (OpCodes.Isinst, eh.CatchType));
				}
				
				var skip = il.Create (OpCodes.Nop);
				
				// check if our epc bit is set
				block.Add (il.Create (OpCodes.Ldarg_0));
				block.Add (il.Create (OpCodes.Ldfld, epcFld));
				block.Add (il.Create (OpCodes.Ldc_I8, 1L << i));
				block.Add (il.Create (OpCodes.And));
				block.Add (il.Create (OpCodes.Brfalse, skip));
				
				// clear epc bits as we go
				block.Add (il.Create (OpCodes.Ldarg_0));
				block.Add (il.Create (OpCodes.Ldarg_0));
				block.Add (il.Create (OpCodes.Ldfld, epcFld));
				block.Add (il.Create (OpCodes.Ldc_I8, ~(1L << i)));
				block.Add (il.Create (OpCodes.And));
				block.Add (il.Create (OpCodes.Stfld, epcFld));
				
				switch (eh.HandlerType) {

				case ExceptionHandlerType.Catch:
					
					// guard by exception type (only if it's more specifc than System.Exception)
					if (eh.CatchType.FullName != "System.Exception") {
						block.Add (il.Create (OpCodes.Dup));
						block.Add (il.Create (OpCodes.Isinst, eh.CatchType));
						block.Add (il.Create (OpCodes.Brfalse, skip));
						block.Add (il.Create (OpCodes.Isinst, eh.CatchType));
					}
					
					block.AddRange (handlerBody);
					
					block.Add (skip);
					catchBlocks.Add (block);
					break;

				//FIXME: Finally blocks need to be surrounded in try..catch!
				case ExceptionHandlerType.Finally:
					
					if (skipFinallyBlocks == null) {
						skipFinallyBlocks = new VariableDefinition (module.TypeSystem.Boolean);
						coroutine.Body.Variables.Add (skipFinallyBlocks);
					}
					
					// replace the endfinally with a ret (patching jumps of course :)
					subsequent = il.Create (OpCodes.Ret);
					handlerBody.RedirectJumps (handlerBody [handlerBody.Count - 1], subsequent);
					handlerBody.RemoveAt (handlerBody.Count - 1);
					handlerBody.Add (subsequent);
					
					// finalize our finally method
					var fil = finallyMethod.Body.GetILProcessor ();
					foreach (var inst in handlerBody) {
						fil.Append (inst);
						if (inst.OpCode == OpCodes.Dup && dupLoc != null)
							finallyMethod.Body.Variables.Add (dupLoc);
					}

					block.Add (il.Create (OpCodes.Ldarg_0));
					block.Add (il.Create (OpCodes.Call, finallyMethod));
					block.Add (skip);
					finallyBlocks.Add (block);
					break;
				
				default:
					throw new NotImplementedException ("Exception handler blocks of type: " + eh.HandlerType.ToString ());
				}
				
				method.Body.ExceptionHandlers.Remove (eh);
				i++;
			}
			
			
			// add state machine
			var firstInst = il.Create (OpCodes.Ldarg_0);
			var loadPC = il.Create (OpCodes.Ldfld, pcFld);
			
			il.Append (firstInst);
			il.Append (loadPC);
			
			// add continuation jump table 
			var jumpTable = new Mono.Cecil.Cil.Instruction [continuations.Count + 1];
			jumpTable [0] = InstructionMap.First (kv => kv.Value.Count != 0).Value [0];
			
			// add mapped new instructions
			foreach (var ci in InstructionMap) {
				foreach (var inst in ci.Value)
					il.Append (inst);
			}
			
			// create global catch/fault blocks
			var ehFirst = il.Create (OpCodes.Nop);
			il.Append (ehFirst);
			
			foreach (var block in catchBlocks)
				foreach (var inst in block)
					il.Append (inst);
			
			il.Emit (OpCodes.Stloc, exception);
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldloc, exception);
			il.Emit (OpCodes.Callvirt, module.Import (setException));

			// if method is void-returning, we gotta throw it
			if (method.ReturnType.IsVoid ()) {
				il.Emit (OpCodes.Ldloc, exception);
				il.Emit (OpCodes.Throw);

			}

			il.Emit (OpCodes.Leave, ret);
			
			var globalEh = new ExceptionHandler (ExceptionHandlerType.Catch) {
				CatchType = exceptionType,
				TryStart = firstInst,
				TryEnd = ehFirst,
				HandlerStart = ehFirst
			};
			coroutine.Body.ExceptionHandlers.Add (globalEh);
			
			var catchEhFirst = il.Create (OpCodes.Stloc, exception);
			
			var catchEh = new ExceptionHandler (ExceptionHandlerType.Catch) {
				CatchType = exceptionType,
				TryStart = ehFirst,
				TryEnd = catchEhFirst,
				HandlerStart = catchEhFirst,
			};
			coroutine.Body.ExceptionHandlers.Add (catchEh);
			
			il.Append (catchEhFirst);
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ldloc, exception);
			il.Emit (OpCodes.Callvirt, module.Import (setException));

			// if method is void-returning, we gotta throw it
			if (method.ReturnType.IsVoid ()) {
				il.Emit (OpCodes.Ldloc, exception);
				il.Emit (OpCodes.Throw);

			}
			
			il.Emit (OpCodes.Leave, ret);

			if (finallyBlocks.Any ()) {
				
				ehFirst = il.Create (OpCodes.Ldloc, skipFinallyBlocks);
				il.Append (ehFirst);
				globalEh.HandlerEnd = ehFirst;
				catchEh.HandlerEnd = ehFirst;

				var endFinally = il.Create (OpCodes.Endfinally);
				il.Emit (OpCodes.Brtrue, endFinally);
				
				foreach (var block in finallyBlocks)
					foreach (var inst in block)
						il.Append (inst);
				
				il.Append (endFinally);
				globalEh = new ExceptionHandler (ExceptionHandlerType.Finally) {
					TryStart = firstInst,
					TryEnd = ehFirst,
					HandlerStart = ehFirst
				};
				coroutine.Body.ExceptionHandlers.Add (globalEh);
			} else {
				catchEh.HandlerEnd = ret;
			}

			globalEh.HandlerEnd = ret;
			il.Append (ret);

			// add continuations
			i = 1;
			foreach (var continuation in continuations) {
				// set the next PC before the jump
				var inst = il.Create (OpCodes.Ldarg_0);
				
				// FIXME: Not as efficient as possible.. since continuation is a Ret we inserted, we had to redir jumps to it previously
				MapInstructions (continuation, inst);
				
				il.InsertBefore (continuation, inst);
				il.InsertBefore (continuation, il.Create (OpCodes.Ldc_I4, i));
				il.InsertBefore (continuation, il.Create (OpCodes.Stfld, pcFld));
				jumpTable [i++] = continuation.Next;
				
				if (skipFinallyBlocks != null) {
					il.InsertBefore (continuation, il.Create (OpCodes.Ldc_I4_1));
					il.InsertBefore (continuation, il.Create (OpCodes.Stloc, skipFinallyBlocks));
				}
				il.Replace (continuation, il.Create (OpCodes.Leave, ret));
			}
			il.InsertAfter (loadPC, il.Create (OpCodes.Switch, jumpTable));
			
			coroutine.Body.InitLocals = true;
			coroutine.Body.ComputeOffsetsAndMaxStack ();
		}
		
		protected virtual void RewriteOriginalMethodBody (TypeDefinition coroutineType, MethodDefinition coroutineCtor)
		{
			
			// If this is a ctor, we need to preserve the base ctor call
			// FIXME: Will this always come first? In C# yes, but other languages... maybe not.
			if (method.IsConstructor) {
				int i = -1;
				int removeIndex = -1;
				while (method.Body.Instructions.Count > removeIndex) {
					i++;
					if (removeIndex == -1 && method.Body.Instructions [i].OpCode.Code == Code.Call) {
						removeIndex = i + 1;
						continue;
					}
					if (removeIndex != -1)
						method.Body.Instructions.RemoveAt (removeIndex);
				}
			} else {
				method.Body.Instructions.Clear ();
			}
			method.Body.Variables.Clear ();
			
			//if (debug)
			//	method.CustomAttributes.Add (new CustomAttribute (module.Import (debuggerHidden)));
			
			var il = method.Body.GetILProcessor ();
			
			var arg = 0;
			if (method.HasThis) {
				arg = 1;
				il.Emit (OpCodes.Ldarg_0);
			}
			
			for (var i = 0; i < method.Parameters.Count; i++)
				il.Emit (OpCodes.Ldarg, arg++);
			
			if (method.HasGenericParameters || method.DeclaringType.HasGenericParameters) {
				
				var genericCtor = coroutineCtor.MakeGeneric (module.Import (coroutineType.MakeGeneric (method.DeclaringType.GenericParameters.Concat (method.GenericParameters).ToArray ())));
				il.Emit (OpCodes.Newobj, module.Import (genericCtor));
				
			} else {
				il.Emit (OpCodes.Newobj, coroutineCtor);
			}
			
			if (method.ReturnType.IsVoid ())
				il.Emit (OpCodes.Pop);
			il.Emit (OpCodes.Ret);
			
			method.Body.ComputeOffsetsAndMaxStack ();
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnLdarg_0 (Instruction instruction)
		{
			return ImplementLdarg (0);
		}
		
		public override IEnumerable<Instruction> OnLdarg_1 (Instruction instruction)
		{
			return ImplementLdarg (1);
		}
		
		public override IEnumerable<Instruction> OnLdarg_2 (Instruction instruction)
		{
			return ImplementLdarg (2);
		}
		
		public override IEnumerable<Instruction> OnLdarg_3 (Instruction instruction)
		{
			return ImplementLdarg (3);
		}
		
		public override IEnumerable<Instruction> OnLdarg (Instruction instruction)
		{
			var param = instruction.Operand as ParameterReference;
			return ImplementLdarg (param != null ? method.IsStatic? param.Index : param.Index + 1 : (int)instruction.Operand);
		}
		
		protected virtual IEnumerable<Instruction> ImplementLdarg (int opr)
		{
			return new Instruction [] {
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Ldfld, argsFields [opr])
			};
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnLdarga (Instruction instruction)
		{
			var param = (ParameterReference)instruction.Operand;
			return ImplementLdarga (method.IsStatic? param.Index : param.Index + 1);
		}
		
		protected virtual IEnumerable<Instruction> ImplementLdarga (int opr)
		{
			return new Instruction [] {
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Ldflda, argsFields [opr])
			};
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnStarg (Instruction instruction)
		{
			var param = instruction.Operand as ParameterReference;
			return ImplementStarg (instruction, param != null ? method.IsStatic? param.Index : param.Index + 1 : (int)instruction.Operand);
		}
		
		protected virtual IEnumerable<Instruction> ImplementStarg (Instruction instruction, int opr)
		{
			var expectedType = argsFields [opr].FieldType;
			InsertLdarg0BeforeLastStackItemAt (instruction);
			
			foreach (var inst in HandleDupsPre (instruction, expectedType))
				yield return inst;
			         
			yield return Instruction.Create (OpCodes.Stfld, argsFields [opr]);
			         
			foreach (var inst in HandleDupsPost (instruction, expectedType))
				yield return inst;
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnLdloc_0 (Instruction instruction)
		{
			return ImplementLdloc (0);
		}
		
		public override IEnumerable<Instruction> OnLdloc_1 (Instruction instruction)
		{
			return ImplementLdloc (1);
		}
		
		public override IEnumerable<Instruction> OnLdloc_2 (Instruction instruction)
		{
			return ImplementLdloc (2);
		}
		
		public override IEnumerable<Instruction> OnLdloc_3 (Instruction instruction)
		{
			return ImplementLdloc (3);
		}
		
		public override IEnumerable<Instruction> OnLdloc (Instruction instruction)
		{
			return ImplementLdloc (((VariableDefinition)instruction.Operand).Index);
		}
		
		protected virtual IEnumerable<Instruction> ImplementLdloc (int opr)
		{
			return new Instruction [] {
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Ldfld, localsFields [opr])
			};
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnLdloca (Instruction instruction)
		{
			return ImplementLdloca (((VariableDefinition)instruction.Operand).Index);
		}
		
		protected virtual IEnumerable<Instruction> ImplementLdloca (int opr)
		{
			return new Instruction [] {
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Ldflda, localsFields [opr])
			};
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnStloc_0 (Instruction instruction)
		{
			return ImplementStloc (instruction, 0);
		}
		
		public override IEnumerable<Instruction> OnStloc_1 (Instruction instruction)
		{
			return ImplementStloc (instruction, 1);
		}
		
		public override IEnumerable<Instruction> OnStloc_2 (Instruction instruction)
		{
			return ImplementStloc (instruction, 2);
		}
		
		public override IEnumerable<Instruction> OnStloc_3 (Instruction instruction)
		{
			return ImplementStloc (instruction, 3);
		}
		
		public override IEnumerable<Instruction> OnStloc (Instruction instruction)
		{
			return ImplementStloc (instruction, ((VariableDefinition)instruction.Operand).Index);
		}
		
		protected virtual IEnumerable<Instruction> ImplementStloc (Instruction instruction, int opr)
		{
			var expectedType = localsFields [opr].FieldType;
			InsertLdarg0BeforeLastStackItemAt (instruction);
			
			foreach (var inst in HandleDupsPre (instruction, expectedType))
				yield return inst;
			
			yield return Instruction.Create (OpCodes.Stfld, localsFields [opr]);
			
			foreach (var inst in HandleDupsPost (instruction, expectedType))
				yield return inst;
	
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnRet (Instruction instruction)
		{
			return ImplementRet (instruction);
		}
		
		protected virtual IEnumerable<Instruction> ImplementRet (Instruction instruction)
		{
			if (method.ReturnType.IsGenericInstance) {
				InsertLdarg0BeforeLastStackItemAt (instruction);
				
				// FIXME: This is a bit shaky, but allows you to return a Future<T> as well as a T from the method
				//  AND is an important fix for when we can't remove the op_Implicit when just returning a T
				if (instruction.Previous.OpCode != OpCodes.Call || ((MethodReference)instruction.Previous.Operand).Name != "op_Implicit") {
					//foreach (var inst in ImplementWait (instruction, method.ReturnType))
					//	yield return inst;
					// The above doesn't play nice with the InsertLdarg0BeforeLastStackItemAt call above
					yield return Instruction.Create (OpCodes.Call, module.ImportFrom (getValue, method.ReturnType.CopyGeneric (coroutineType, skews)));
				}

				yield return Instruction.Create (OpCodes.Call, module.ImportFrom (setValue, method.ReturnType.CopyGeneric (coroutineType, skews)));
				
			} else {
				if (!method.ReturnType.IsVoid ())
					yield return Instruction.Create (OpCodes.Pop);
				
				yield return Instruction.Create (OpCodes.Ldarg_0);
				yield return Instruction.Create (OpCodes.Ldc_I4_1);
				yield return Instruction.Create (OpCodes.Call, module.Import (setStatus));
			}
			
			yield return Instruction.Create (OpCodes.Leave, ret);
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnLeave (Instruction instruction)
		{
			return ImplementLeave (instruction);
		}
		
		protected virtual IEnumerable<Instruction> ImplementLeave (Instruction instruction)
		{
			// ONLY for leave instructions NOT within a handler block:
			// remove if this just points to the instruction following the exception handler
			
			var query = from eh in method.Body.ExceptionHandlers
			            where eh.TryStart.Offset <= instruction.Offset && eh.TryEnd.Offset > instruction.Offset
			            let lastHandler = method.Body.ExceptionHandlers.OrderBy (h => h.HandlerStart.Offset).LastOrDefault ((l, c) => l.HandlerEnd == c.HandlerStart, eh)
			            where (Instruction)instruction.Operand == lastHandler.HandlerEnd
			            select eh;
			
			if (!query.Any () || method.Body.ExceptionHandlers.Any (eh => eh.HandlerStart.Offset <= instruction.Offset && eh.HandlerEnd.Offset > instruction.Offset))
				return null;
			
			MapInstructions (instruction, (Instruction)instruction.Operand);
			return new Instruction [0];
		}
		
		// -----
		
		public override IEnumerable<Instruction> OnCall (Instruction instruction)
		{
			// FIXME: Might not be first thing in method
			if (method.IsConstructor && ((MethodReference)instruction.Operand).Name == ".ctor") {
				InstructionMap.Clear ();
				return new Instruction [0];
			}
			
			return ImplementCall (instruction);
		}
		
		public override IEnumerable<Instruction> OnCallvirt (Instruction instruction)
		{
			return ImplementCall (instruction);
		}
		
		protected virtual IEnumerable<Instruction> ImplementCall (Instruction instruction)
		{
			var callee = instruction.Operand as MethodReference;
			if (callee.Name == "Wait" && callee.DeclaringType.IsFutureType ()) {
				
				return ImplementWait (instruction, callee.DeclaringType as GenericInstanceType);
			}
			
			if (callee.Name == "op_Implicit" && callee.DeclaringType.IsBuiltInFutureType ()
				&& instruction.Next.OpCode == OpCodes.Ret) // < FIXME: This does not work with MS compiler debug build
				return new Instruction [0];
			
			if (callee.Name == "Yield" && callee.DeclaringType.FullName == "Cirrus.Thread") {
				return ImplementYield ();
			}
			
			instruction.Operand = FixMethodToken (callee);
			return null;
		}
		
		protected virtual IEnumerable<Instruction> ImplementWait (Instruction instruction, TypeReference waitFutureType)
		{
			//FIXME: We're reordering instructions to keep the stack balanced before/after continuation.
			// Better to save/restore the stack instead? ->
			//  Foo (MayThrow (), GetSomeFuture ().Wait ());
			// Normally, if MayThrow () throws an exception, GetSomeFuture () is not called. But because we're
			// reordering, GetSomeFuture *is* called, the future is waited, and then MayThrow throws on the continuation.
										
			//FIXME: Support multiple predecessors here!
			var lastStack = cfg.FindLastStackItem (block, instruction, IsBarrier).Single (); // this is the Future
			// these are the items on the stack before the Future. they will be moved til after the continuation
			var continuationInst = cfg.FindStackHeight (block, lastStack, 0, IsBarrier).Single ();
			
			IList<Instruction> precontinuationStack = null;
			if (continuationInst != lastStack) {
				precontinuationStack = ExtractInstructionRange (continuationInst, lastStack);
				
				// FIXME: Currently, we can't handle any jumps into this critical area
				if (precontinuationStack.Any (i => method.Body.IsJumpTarget (i)))
					throw new NotSupportedException ("Support for jumps into continuation critical area not implemented");
			}
			
			// Insert ldarg.0 for CoroutineFuture.Chain
			// FIXME: Right now, we're depending on the C# compiler's behavior re. not separating arg0 from call
			InsertBefore (lastStack, Instruction.Create (OpCodes.Ldarg_0));
			
			var chainInstructions = new List<Instruction> ();
			chainInstructions.Add (Instruction.Create (OpCodes.Call, chain));
			
			var continuation = Instruction.Create (OpCodes.Ret);
			continuations.Add (continuation);
			
			var afterContinuation = Instruction.Create (OpCodes.Ldarg_0);
			
			// CoroutineFuture.Chain returns false if we don't need continuation
			chainInstructions.Add (Instruction.Create (OpCodes.Brfalse_S, afterContinuation));
			
			chainInstructions.Add (continuation);
			chainInstructions.Add (afterContinuation);
			
			// this is the instruction that will follow if the future is not faulted
			var notFaulted = Instruction.Create (OpCodes.Nop);
			
			// check for exceptions
			chainInstructions.Add (Instruction.Create (OpCodes.Call, checkException));
			chainInstructions.Add (Instruction.Create (OpCodes.Ldarg_0));
			chainInstructions.Add (Instruction.Create (OpCodes.Ldfld, chainedFld));
			chainInstructions.Add (Instruction.Create (OpCodes.Callvirt, module.Import (getException)));
			chainInstructions.Add (Instruction.Create (OpCodes.Brfalse, notFaulted));
			
			// mark exception as handled and throw it again right there
			chainInstructions.Add (Instruction.Create (OpCodes.Ldarg_0));
			chainInstructions.Add (Instruction.Create (OpCodes.Ldfld, chainedFld));
			chainInstructions.Add (Instruction.Create (OpCodes.Dup));
			chainInstructions.Add (Instruction.Create (OpCodes.Ldc_I4, -1));
			chainInstructions.Add (Instruction.Create (OpCodes.Call, module.Import (setStatus)));
			chainInstructions.Add (Instruction.Create (OpCodes.Callvirt, module.Import (getException)));
			chainInstructions.Add (Instruction.Create (OpCodes.Throw));
			
			// redirect jumps from our phony Wait call to the chain call.
			method.Body.Instructions.RedirectJumps (instruction, chainInstructions [0]);
				
			// -- add continuation to InstructionMap
			InstructionMap.Add (chainInstructions [0], chainInstructions);

			yield return notFaulted;

			// move anything on the stack before the Wait to after the continuation
			if (precontinuationStack != null) {
				foreach (var inst in precontinuationStack)
					yield return inst;
			
			}
			
			// load the continuation result if there is one
			
			// FIXME: Eventually, we prolly don't want to do this if we're just going to pop the result..
			//   but we'd have to get rid of the pop instruction too...
			if (waitFutureType != null /* && instruction.Next.OpCode != OpCodes.Pop */) {
				var genWaitFuture = waitFutureType.CopyGeneric (coroutineType, skews);
				yield return Instruction.Create (OpCodes.Ldarg_0);
				yield return Instruction.Create (OpCodes.Ldfld, chainedFld);
				yield return Instruction.Create (OpCodes.Isinst, genWaitFuture);
				yield return Instruction.Create (OpCodes.Call, module.ImportFrom (getValue, genWaitFuture));
			}
			
			// reset chained to null
			yield return Instruction.Create (OpCodes.Ldarg_0);
			yield return Instruction.Create (OpCodes.Ldnull);
			yield return Instruction.Create (OpCodes.Stfld, chainedFld);
		}
		
		protected IEnumerable<Instruction> ImplementYield ()
		{
			var inst = Instruction.Create (OpCodes.Ret);
			continuations.Add (inst);

			return new Instruction [] {
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Ldfld, threadFld),
				Instruction.Create (OpCodes.Call, module.Import (schedule)),
				inst,
				Instruction.Create (OpCodes.Ldarg_0),
				Instruction.Create (OpCodes.Call, checkException)
			};
		}
		
		// -----
		
		//FIXME: Theoretically we need to do this for any op that emits a type 
		public override IEnumerable<Instruction> OnInitobj (Instruction instruction)
		{
			instruction.Operand = FixTypeToken ((TypeReference)instruction.Operand);
			return null;
		}

		public override IEnumerable<Instruction> OnNewobj (Instruction instruction)
		{
			instruction.Operand = FixMethodToken ((MethodReference)instruction.Operand);
			return null;
		}

		public override IEnumerable<Instruction> OnConstrained (Instruction instruction)
		{
			instruction.Operand = FixTypeToken ((TypeReference)instruction.Operand);
			return null;
		}

		public override IEnumerable<Instruction> OnLdtoken (Instruction instruction)
		{
			instruction.Operand = FixTypeToken ((TypeReference)instruction.Operand);
			return null;
		}

		public override IEnumerable<Instruction> OnUnbox_Any (Instruction instruction)
		{
			instruction.Operand = FixTypeToken ((TypeReference)instruction.Operand);
			return null;
		}

		// -----
		
		// We are taking a method implementation and extracting it into its own class.
		// If the method is generic, we need to make that class generic and update any
		// type refs to the old method's generic parameters to refer to the new class's
		// generic parameters instead.

		protected TypeReference FixTypeToken (TypeReference token)
		{
			return module.Import (token.CopyGeneric (coroutineType, skews));
		}
		
		protected MethodReference FixMethodToken (MethodReference token)
		{
			return module.Import (token.CopyGeneric (module, coroutineType, skews));
		}
		
		// -----
		
		// We convert locals and arguments into a fields.
		// The stfld instruction requires an instance pointer, while stloc and starg do not.
		// We insert that instance pointer as a Ldarg_0 instruction using the CFG to find
		// the correct stack height. This usually works okay, but will screw things up if
		// there is a Dup instruction between our inserted Ldarg_0 and our stfld.
		// Remedy this by popping the dup value into a temp local and pushing it again after our stfld.
		
		protected IEnumerable<Instruction> HandleDupsPre (Instruction instruction, TypeReference expectedType)
		{
			if (instruction.Previous.OpCode != OpCodes.Dup)
				yield break;
			
			if (dupLoc == null) {
				dupLoc = new VariableDefinition (module.TypeSystem.Object);
				coroutine.Body.Variables.Add (dupLoc);
			}
			
			if (expectedType.IsValueType)
				yield return Instruction.Create (OpCodes.Box, expectedType);
			
			yield return Instruction.Create (OpCodes.Stloc, dupLoc);
			
		}
		
		protected IEnumerable<Instruction> HandleDupsPost (Instruction instruction, TypeReference expectedType)
		{
			if (instruction.Previous.OpCode != OpCodes.Dup)
				yield break;
			
			yield return Instruction.Create (OpCodes.Ldloc, dupLoc);
			if (expectedType.IsValueType) {
				yield return Instruction.Create (OpCodes.Unbox, expectedType);
				yield return Instruction.Create (OpCodes.Ldobj, expectedType);
			}
		}
		
	}
}

