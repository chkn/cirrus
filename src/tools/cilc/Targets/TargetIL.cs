/*
	TargetIL.cs: Cirrus Fibers Coroutine Transformation: CIL-to-CIL
   
	Copyright (c) 2010 Alexander Corrado
  
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecil.Decompiler.Cil;

using Cirrus.Tools.Cilc.Util;

namespace Cirrus.Tools.Cilc.Targets {
	public class TargetIL : Target {	
		
		// FYI, this indicates all the places where continuations will be inserted (and must be updated accordingly :)
		private static bool IsBarrier (Instruction instruction)
		{
			if (instruction.OpCode.FlowControl != FlowControl.Call)
					return false;
				
			var callee = instruction.Operand as MethodReference;
			
			return (callee.Name == "Wait" && IsFutureType (callee.DeclaringType)) ||
			       (callee.Name == "Sleep" && callee.DeclaringType.FullName == "Cirrus.Thread") ||
			       (callee.Name == "get_Current" && IsAsyncEnumeratorType (callee.DeclaringType));
		}
		
		// Returns true if method was modified
		public override bool ProcessMethod (MethodDefinition method)
		{	
			base.ProcessMethod (method);
			if (!method.HasBody)
				return false;
			
			// Look for one of the 3 IL trigger patterns:
			//  1) Call Future (or subclass) Wait ()
			//  2) Call Thread.Sleep
			//  3) Call AsyncEnumerator get_Current
			
			bool transformed = false;
			var returnType = method.ReturnType;
			
			foreach (var instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl != FlowControl.Call)
					continue;
				
				var callee = instruction.Operand as MethodReference;
				
				//1
				if (callee.Name == "Wait" && IsFutureType (callee.DeclaringType)) {
					if (!IsBuiltInFutureType (returnType))
						throw new Error (method.Module.Name,
						                 string.Format ("method `{0}' in type `{1}' uses asynchronous Wait, but its return type is not Cirrus.Future or Cirrus.Future<T>",
						                                method.Name, method.DeclaringType.Name));
					
					TransformAsyncMethod (method);
					transformed = true;
					break;
				}
				
				//2
				if (callee.Name == "Sleep" && callee.DeclaringType.FullName == "Cirrus.Thread") {
					if (!IsBuiltInFutureType (returnType))
						throw new Error (method.Module.Name,
						                 string.Format ("method `{0}' in type `{1}' uses Thread.Sleep, but its return type is not Cirrus.Future or Cirrus.Future<T>",
						                                method.Name, method.DeclaringType.Name));

					TransformAsyncMethod (method);
					transformed = true;
					break;
				}
				
				//3
				if (callee.Name == "get_Current" && IsAsyncEnumeratorType (callee.DeclaringType)) {
					if (!IsBuiltInFutureType (returnType))
						throw new Error (method.Module.Name,
						                 string.Format ("method `{0}' in type `{1}' uses an asynchronous foreach, but its return type is not Cirrus.Future or Cirrus.Future<T>",
						                                method.Name, method.DeclaringType.Name));
					
					TransformAsyncMethod (method);
					transformed = true;
					break;
				}
					
			}
			
			return transformed;
		}
		
		public override void SaveOutput (ModuleDefinition module, string fileName)
		{
			module.Write (fileName);
		}
		
		public override void CreatePlatformExecutible (string name)
		{
		}
		
		protected virtual void TransformAsyncMethod (MethodDefinition method)
		{
			if (method.Parameters.Any (p => p.IsOut))
				throw new Error (method.Module.Name,
				                 string.Format ("method `{0}' in type `{1}' cannot be transformed into an asynchronous coroutine becuase it has a ref or out parameter",
				                                method.Name, method.DeclaringType.Name)); 
			
			// Create CoroutineFuture implementation...
			var module = method.Module;
			var containingType = method.DeclaringType;
			var returnType = method.ReturnType;
			
			var voidType = module.Import (typeof (void));
			
			FieldDefinition [] argsFields;
			FieldDefinition [] localsFields;
			
			TypeReference futureValueType = null;
			TypeReference baseType = null;
			FieldReference chainedFld = null;
			FieldReference pcFld = null;
			MethodReference baseCtor = null;
			
			VariableDefinition sleepLoc = null;
			VariableDefinition dupLoc = null;
			
			if (returnType.IsGenericInstance) { // returns Future<T>
				futureValueType = ((GenericInstanceType)returnType).GenericArguments [0];
				baseType = module.Import (typeof (Cirrus.CoroutineFuture<>)).MakeGeneric (futureValueType);
				chainedFld = module.Import (typeof (Cirrus.CoroutineFuture<>).GetField ("chained", BindingFlags.Instance | BindingFlags.NonPublic), baseType);
				chainedFld.DeclaringType = baseType;
				pcFld = module.Import (typeof (Cirrus.CoroutineFuture<>).GetField ("pc", BindingFlags.Instance | BindingFlags.NonPublic), baseType);
				pcFld.DeclaringType = baseType;
				baseCtor = module.Import (typeof (Cirrus.CoroutineFuture<>).GetConstructor (new Type [] {}), baseType);
				baseCtor.DeclaringType = baseType;
				
			} else { // returns Future...
				baseType = module.Import (typeof (Cirrus.CoroutineFuture));
				chainedFld = module.Import (typeof (Cirrus.CoroutineFuture).GetField ("chained", BindingFlags.Instance | BindingFlags.NonPublic));
				pcFld = module.Import (typeof (Cirrus.CoroutineFuture).GetField ("pc", BindingFlags.Instance | BindingFlags.NonPublic));
				baseCtor = module.Import (typeof (Cirrus.CoroutineFuture).GetConstructor (new Type [] {}));
			}
			
			var futureName = string.Format ("__cirrus_{0}_{1}_impl", method.Name, string.Join ("_", method.Parameters.Select (p => p.ParameterType.Name).ToArray ()));
			
			var future = new TypeDefinition (null, futureName, Mono.Cecil.TypeAttributes.NestedPrivate | Mono.Cecil.TypeAttributes.Sealed, baseType);
			containingType.NestedTypes.Add (future);
			
			var ctor = new MethodDefinition (".ctor", Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName |
			                                   Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Public, voidType);
			var ctorIL = ctor.Body.GetILProcessor ();
			future.Methods.Add (ctor);
			
			// first, call base ctor
			ctorIL.Emit (OpCodes.Ldarg_0);
			ctorIL.Emit (OpCodes.Call, baseCtor);
			
			// add "this"
			int argStart = 0;
			if (method.HasThis) {
				argsFields = new FieldDefinition [method.Parameters.Count+1];
				argsFields [0] = new FieldDefinition ("$this", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.InitOnly, containingType);
				future.Fields.Add (argsFields [0]);
				ctor.Parameters.Add (new ParameterDefinition (containingType));
				
				// this.$this = <Arg1>
				ctorIL.Emit (OpCodes.Ldarg_0);
				ctorIL.Emit (OpCodes.Ldarg_1);
				ctorIL.Emit (OpCodes.Stfld, argsFields [0]);
				
				argStart = 1;
			} else {
				argsFields = new FieldDefinition [method.Parameters.Count];	
			}
			
			// load all args
			int i = argStart;
			foreach (var arg in method.Parameters) {
				argsFields [i] = new FieldDefinition ("$arg" + i, Mono.Cecil.FieldAttributes.Private, arg.ParameterType);
				future.Fields.Add (argsFields [i]);
				ctor.Parameters.Add (new ParameterDefinition (arg.ParameterType));
				
				// this.$argX = <ArgX>
				ctorIL.Emit (OpCodes.Ldarg_0);
				ctorIL.Emit (OpCodes.Ldarg, i + 1);
				ctorIL.Emit (OpCodes.Stfld, argsFields [i]);
				
				i++;
			}
			
			// create a field for each local
			i = 0;
			localsFields = new FieldDefinition [method.Body.Variables.Count];
			foreach (var local in method.Body.Variables) {
				localsFields [i] = new FieldDefinition ("$loc" + i, Mono.Cecil.FieldAttributes.Private, local.VariableType);
				future.Fields.Add (localsFields [i]);
				i++;
			}
			
			// create coroutine method
			var coroutine = new MethodDefinition ("Coroutine", Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Family |
			                                      Mono.Cecil.MethodAttributes.Virtual, voidType);
			future.Methods.Add (coroutine);
			
			var il = coroutine.Body.GetILProcessor ();
			var cfg = ControlFlowGraph.Create (method);
		
			var continuations = new List<Instruction> ();
			var coroutineInstructions = new OrderedDictionary<Instruction, List<Instruction>> (); // of (original Instruction) -> (List of replacement Instructions)
			
			// process the method we want to transform and...
			//  1) Replace all Ldarg, Starg opcodes with Ldfld, Stfld (argsFields)
			//  2) Replace all Ldloc, Stloc opcodes with Ldfld, Stfld (localsFields)
			//  3) Before Ret, add Future<T>.Value = ...; or Future.Status = FutureStatus.Fulfilled;
			//  4) Replace calls to Future.Wait or AsyncEnumerator.get_Current with continuation
			//  5) Remove calls to Future<T>.op_Implicit preceeding Ret
			//  6) Replace calls to Thread.Sleep with continuation
			foreach (var block in cfg.Blocks) {
				foreach (var instruction in block) { 
					
					var current = new List<Instruction> ();
					int? opr = null;
					MethodReference callee = null;
					Instruction lastStack = null;
					
					switch (instruction.OpCode.Value) {
					//1
					case -254 /*OpCodes.Ldarg_0*/: opr = 0; goto case -503;
					case -253 /*OpCodes.Ldarg_1*/: opr = 1; goto case -503;
					case -252 /*OpCodes.Ldarg_2*/: opr = 2; goto case -503;
					case -251 /*OpCodes.Ldarg_3*/: opr = 3; goto case -503;
					case -242 /*OpCodes.Ldarg_S*/:
					case -503 /*OpCodes.Ldarg*/:   opr = opr ?? (int)instruction.Operand;
						current.Add (il.Create (OpCodes.Ldarg_0));
						current.Add (il.Create (OpCodes.Ldfld, argsFields [opr.Value]));
						break;
					
					case -241 /*OpCodes.Ldarga_S*/:
					case -502 /*OpCodes.Ldarga*/:
						current.Add (il.Create (OpCodes.Ldarg_0));
						current.Add (il.Create (OpCodes.Ldflda, argsFields [((ParameterReference)instruction.Operand).Index]));
						break;
						
					case -240 /*OpCodes.Starg_S*/:
					case -501 /*OpCodes.Starg*/: 
						opr = (int)instruction.Operand;
						
						lastStack = cfg.FindLastStackItem (block, instruction, IsBarrier);
						coroutineInstructions [lastStack].Insert (0, il.Create (OpCodes.Ldarg_0));
						
						current.Add (il.Create (OpCodes.Stfld, argsFields [opr.Value]));
						HandleDups (coroutine, coroutineInstructions, instruction, current, argsFields [opr.Value].FieldType, ref dupLoc);
						break;
					
					//2
					case -250 /*OpCodes.Ldloc_0*/: opr = 0; goto case -500;
					case -249 /*OpCodes.Ldloc_1*/: opr = 1; goto case -500;
					case -248 /*OpCodes.Ldloc_2*/: opr = 2; goto case -500;
					case -247 /*OpCodes.Ldloc_3*/: opr = 3; goto case -500;
					case -239 /*OpCodes.Ldloc_S*/:
					case -500 /*OpCodes.Ldloc*/:   opr = opr ?? ((VariableDefinition)instruction.Operand).Index;
						current.Add (il.Create (OpCodes.Ldarg_0));
						current.Add (il.Create (OpCodes.Ldfld, localsFields [opr.Value]));	
						break;
					
					case -238 /*OpCodes.Ldloca_S*/: goto case -499;
					case -499 /*OpCodes.Ldloca*/:
						current.Add (il.Create (OpCodes.Ldarg_0));
						current.Add (il.Create (OpCodes.Ldflda, localsFields [((VariableDefinition)instruction.Operand).Index]));
						break;
					
					case -246 /*OpCodes.Stloc_0*/: opr = 0; goto case -498;
					case -245 /*OpCodes.Stloc_1*/: opr = 1; goto case -498;
					case -244 /*OpCodes.Stloc_2*/: opr = 2; goto case -498;
					case -243 /*OpCodes.Stloc_3*/: opr = 3; goto case -498;
					case -237 /*OpCodes.Stloc_S*/:
					case -498 /*OpCodes.Stloc*/:   opr = opr ?? ((VariableDefinition)instruction.Operand).Index;
						lastStack = cfg.FindLastStackItem (block, instruction, IsBarrier);
						coroutineInstructions [lastStack].Insert (0, il.Create (OpCodes.Ldarg_0));
						
						current.Add (il.Create (OpCodes.Stfld, localsFields [opr.Value]));	
						HandleDups (coroutine, coroutineInstructions, instruction, current, localsFields [opr.Value].FieldType, ref dupLoc);
						break;
						
					//3
					case -214 /*OpCodes.Ret*/:
						if (returnType.IsGenericInstance) {
							var setValueMethod = module.Import (typeof (Future<>).GetProperty ("Value").GetSetMethod (), returnType);
							setValueMethod.DeclaringType = returnType;
							
							lastStack = cfg.FindLastStackItem (block, instruction, IsBarrier);
							coroutineInstructions [lastStack].Insert (0, il.Create (OpCodes.Ldarg_0));
							current.Add (il.Create (OpCodes.Call, setValueMethod));
						} else {
							var setStatusMethod = module.Import (typeof (Future).GetProperty ("Status").GetSetMethod ());
							
							lastStack = cfg.FindLastStackItem (block, instruction, IsBarrier);
							coroutineInstructions [lastStack].Insert (0, il.Create (OpCodes.Ldarg_0));
							current.Add (il.Create (OpCodes.Pop));
							current.Add (il.Create (OpCodes.Ldc_I4_1));
							current.Add (il.Create (OpCodes.Call, setStatusMethod));
						}
						current.Add (il.Create (OpCodes.Ret));
						break;
					
					
					case -145 /*OpCodes.Callvirt*/:
					case -216 /*OpCodes.Call*/:
						callee = instruction.Operand as MethodReference;
						//4
						if ((callee.Name == "Wait" && IsFutureType (callee.DeclaringType)) ||
						    (callee.Name == "get_Current" && IsAsyncEnumeratorType (callee.DeclaringType))) {
							//FIXME: We're reordering instructions to keep the stack balanced before/after continuation.
							// Better to save/restore the stack instead? ->
							//  Foo (MayThrow (), GetSomeFuture ().Wait ());
							// Normally, if MayThrow () throws an exception, GetSomeFuture () is not called. But because we're
							// reordering, GetSomeFuture *is* called, the future is waited, and then MayThrow throws on the continuation.
							
							lastStack = cfg.FindLastStackItem (block, instruction, IsBarrier); // this is the Future
							// these are the items on the stack before the Future. they will be moved til after the continuation
							var continuationInst = cfg.FindStackHeight (block, lastStack, 0, IsBarrier);
							
							// emit this to setfld "chained"
							// FIXME: Right now, we're depending on the C# compiler's behavior re. not separating arg0 from call
							coroutineInstructions [lastStack].Insert (0, il.Create (OpCodes.Ldarg_0));
							
							current.Add (il.Create (OpCodes.Stfld, chainedFld));
							var inst = il.Create (OpCodes.Ret);
							continuations.Add (inst);
							current.Add (inst);
							coroutineInstructions.Add (inst, current);
							
							i = coroutineInstructions.IndexOfKey (continuationInst);
							while (continuationInst != lastStack) {
								// move these to after the continuation
								// FIXME: Currently, we can't handle any jumps into this critical area
								if (method.Body.IsJumpTarget (continuationInst))
									throw new NotSupportedException ("Support for jumps into continuation critical area not implemented");
								
								current = coroutineInstructions [i];
								coroutineInstructions.RemoveAt (i);
								coroutineInstructions.Add (continuationInst, current);
								
								continuationInst = coroutineInstructions.KeyForIndex (++i);
							}
							
							// load the continuation result if there is one
							var waitFutureType = callee.DeclaringType as GenericInstanceType;
							if (waitFutureType != null) {
								var getValueMethod = module.Import (typeof (Future<>).GetProperty ("Value").GetGetMethod (), waitFutureType);
								getValueMethod.DeclaringType = waitFutureType;
								
								current = new List<Instruction> ();
								inst = il.Create (OpCodes.Ldarg_0);
								current.Add (inst);
								current.Add (il.Create (OpCodes.Ldfld, chainedFld));
								current.Add (il.Create (OpCodes.Call, getValueMethod));
								coroutineInstructions.Add (instruction, current);
							}
							
							continue;
						}
						
						//5
						if (callee.Name == "op_Implicit" && IsBuiltInFutureType (callee.DeclaringType)
						    && instruction.Next.OpCode == OpCodes.Ret)
							continue;
						
						//FIXME: Any code that jumps directly to Cirrus.Thread.Sleep (past the int arg load)
						//  will screw this up because we need ldarg.0 to make the this.WakeupTime property set work.
						//6
						if (callee.Name == "Sleep" && callee.DeclaringType.FullName == "Cirrus.Thread") {
							// create our dumb local to store the sleep DateTime because it's a valuetype
							if (sleepLoc == null) {
								sleepLoc = new VariableDefinition (module.Import (typeof (DateTime)));
								coroutine.Body.Variables.Add (sleepLoc);
							}
							
							// these will go before the sleep time in the stack
							current.Add (il.Create (OpCodes.Ldarg_0));
							current.Add (il.Create (OpCodes.Call, module.Import (typeof (DateTime).GetProperty ("Now", BindingFlags.Public | BindingFlags.Static).GetGetMethod ())));
							current.Add (il.Create (OpCodes.Stloc, sleepLoc));
							current.Add (il.Create (OpCodes.Ldloca, sleepLoc));
							
							lastStack = cfg.FindLastStackItem (block, instruction, IsBarrier);
							coroutineInstructions [lastStack].InsertRange (0, current);
							
							current.Clear ();
							current.Add (il.Create (OpCodes.Conv_R8));
							current.Add (il.Create (OpCodes.Call, module.Import (typeof (DateTime).GetMethod ("AddMilliseconds"))));
							
							var nullableCtor = module.Import (typeof (Nullable<DateTime>).GetConstructor (new Type [] { typeof (DateTime) }));
							current.Add (il.Create (OpCodes.Newobj, nullableCtor));
							current.Add (il.Create (OpCodes.Call, module.Import (typeof (Future).GetProperty ("WakeupTime").GetSetMethod ())));
							var inst = il.Create (OpCodes.Ret);
							continuations.Add (inst);
							current.Add (inst);
						}
						break;
					}
					
					if (current.Count == 0)
						current.Add (instruction);
					
					coroutineInstructions.Add (instruction, current);
				}
			}
			
			/////////// ACTUAL COROUTINE METHOD IL EMITTING BEGINS HERE:
			
			// add state machine
			il.Emit (OpCodes.Ldarg_0);
			var loadPC = il.Create (OpCodes.Ldfld, pcFld);
			il.Append (loadPC);
			
			var jumpTable = new Mono.Cecil.Cil.Instruction [continuations.Count + 1];
			var kve = ((IEnumerable<KeyValuePair<Instruction,List<Instruction>>>)coroutineInstructions).GetEnumerator ();
			bool first = true;
			
			while (kve.MoveNext ()) {
				var kv = kve.Current;
				var current = kv.Value;
				
				if (current [0] != kv.Key)
					method.Body.RedirectJumps (kv.Key, current [0]);
				
				foreach (var inst in current)	
					il.Append (inst);
				
				if (first) { jumpTable [0] = current.First (); first = false; }
			}
			
			i = 1;
			foreach (var continuation in continuations) {
				// set the next PC before the jump
				il.InsertBefore (continuation, il.Create (OpCodes.Ldarg_0));
				il.InsertBefore (continuation, il.Create (OpCodes.Ldc_I4, i));
				il.InsertBefore (continuation, il.Create (OpCodes.Stfld, pcFld));
				jumpTable [i++] = continuation.Next;
			}
				
			il.InsertAfter (loadPC, il.Create (OpCodes.Switch, jumpTable));
			
			// finalize coroutine
			coroutine.Body.MaxStackSize = coroutine.Body.ComputeOffsetsAndMaxStack ();
			
			// replace original method with a call to the coroutine
			method.Body.Instructions.Clear ();
			method.Body.Variables.Clear ();
			ctor.Body.MaxStackSize = method.Body.MaxStackSize = Math.Max (method.Parameters.Count, 1);
			
			il = method.Body.GetILProcessor ();
			if (method.HasThis) {
				method.Body.MaxStackSize++;
				il.Emit (OpCodes.Ldarg_0);
			}
			
			i = argStart;
			foreach (var arg in method.Parameters)
				il.Emit (OpCodes.Ldarg, i++);
			
			il.Emit (OpCodes.Newobj, ctor);
			il.Emit (OpCodes.Ret);
			
			
			// finish constructor
			ctorIL.Emit (OpCodes.Ldarg_0);
			ctorIL.Emit (OpCodes.Call, coroutine);
			ctorIL.Emit (OpCodes.Ret);
			
			
		}
		
		public static void HandleDups (MethodDefinition coroutine, OrderedDictionary<Instruction, List<Instruction>> coroutineInstructions, Instruction instruction, List<Instruction> current, TypeReference expectedType, ref VariableDefinition dupLoc)
		{
			// FIXME: This is kindof a kludge... think on it
			if (instruction.Previous.OpCode != OpCodes.Dup)
				return;
						
			var action = current.ToArray ();
			current.Clear ();
			
			if (dupLoc == null) {
				dupLoc = new VariableDefinition (coroutine.Module.Import (typeof (object)));
				coroutine.Body.Variables.Add (dupLoc);
			}
			
			if (expectedType.IsValueType)
				current.Add (Instruction.Create (OpCodes.Box, expectedType));
			
			current.Add (Instruction.Create (OpCodes.Stloc, dupLoc));
			
			current.AddRange  (action);
			
			current.Add (Instruction.Create (OpCodes.Ldloc, dupLoc));
			if (expectedType.IsValueType) {
				current.Add (Instruction.Create (OpCodes.Unbox, expectedType));
				current.Add (Instruction.Create (OpCodes.Ldobj, expectedType));
			}
			
		}
		
		public static bool IsAsyncEnumeratorType (TypeReference type)
		{
			if (type == null)
				return false;
						
			return type.FullName.StartsWith ("Cirrus.AsyncEnumerator`1");
		}
		public static bool IsBuiltInFutureType (TypeReference type)
		{
			if (type == null)
				return false;
			
			var typeName = type.FullName;
			return typeName == "Cirrus.Future" || typeName.StartsWith ("Cirrus.Future`1");
		}
		
		public static bool IsFutureType (TypeReference type)
		{
			if (type == null)
				return false;
			
			if (IsBuiltInFutureType (type))
				return true;
			
			return IsFutureType (type.Resolve ().BaseType);
		}
	}
}

