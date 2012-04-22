/*
	CecilHelpers.cs: Static and extension utility methods for Mono.Cecil and Cecil.Decompiler
  
	Copyright (c) 2010 Alexander Corrado
	(C) 2005 - 2007 db4objects Inc. http://www.db4o.com
	(C) 2007 - 2008 Novell, Inc. http://www.novell.com
	(C) 2007 - 2008 Jb Evain http://evain.net
  
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

namespace Cirrus.Tools.Cilc.Util {
	
	//public delegate Instruction StepChecker (Instruction currentInstruction, Instruction nextPreviousInstruction);
	
	public static class CecilHelpers {
		
		public static MethodReference ImportFrom (this ModuleDefinition module, MethodBase method, TypeReference declaringType)
		{
			var imported = module.Import (method, declaringType);
			imported.DeclaringType = declaringType;
			
			return imported;
		}
		public static MethodReference ImportFrom (this ModuleDefinition module, MethodReference method, TypeReference declaringType)
		{
			var imported = module.Import (method, declaringType);
			imported.DeclaringType = declaringType;
			
			return imported;
		}
		
		public static FieldReference ImportFrom (this ModuleDefinition module, FieldInfo field, TypeReference declaringType)
		{
			var imported = module.Import (field, declaringType);
			imported.DeclaringType = declaringType;
			
			return imported;
		}
		public static FieldReference ImportFrom (this ModuleDefinition module, FieldReference field, TypeReference declaringType)
		{
			var imported = module.Import (field, declaringType);
			imported.DeclaringType = declaringType;
			
			return imported;
		}
		
		public static TypeReference MakeGeneric (this TypeReference type, params TypeReference [] args)
		{
			if (args.Length == 0)
				return type;
			
			if (type.GenericParameters.Count != args.Length)
				throw new ArgumentException ("Invalid number of generic type arguments supplied");
			
			var genericTypeRef = new GenericInstanceType (type);
			foreach (var arg in args)
				genericTypeRef.GenericArguments.Add (arg);
			
			return genericTypeRef;
		}
		
		public static MethodReference MakeGeneric (this MethodReference method, params TypeReference [] args)
		{
			if (args.Length == 0)
				return method;
			
			if (method.GenericParameters.Count != args.Length)
				throw new ArgumentException ("Invalid number of generic type arguments supplied");
			
			var genericTypeRef = new GenericInstanceMethod (method);
			foreach (var arg in args)
				genericTypeRef.GenericArguments.Add (arg);
			
			//foreach (var param in genericTypeRef.Parameters) {
			//	param.ParameterType = param.ParameterType.CopyGeneric (genericTypeRef, method);
			//}
			
			return genericTypeRef;
		}
		
		public static MethodReference MakeGeneric (this MethodReference method, TypeReference declaringType)
		{
		    var reference = new MethodReference (method.Name, method.ReturnType, declaringType);
			reference.CallingConvention = method.CallingConvention;
			reference.HasThis = method.HasThis;
			reference.ExplicitThis = method.ExplicitThis;
			
		    foreach (var parameter in method.Parameters)
		        reference.Parameters.Add (new ParameterDefinition (parameter.ParameterType));
		
		    return reference;
		}
	/*	
		public static TypeReference CopyGeneric (this TypeReference type, IGenericInstance newBasis, IGenericParameterProvider oldBasis)
		{
			var genParam = type as GenericParameter;
			if (genParam != null && genParam.Owner == oldBasis) {
				return newBasis.GenericArguments [genParam.Position];
			}
			
			var genInst = type as GenericInstanceType;
			if (genInst != null) {
				
				var args = new TypeReference [genInst.GenericArguments.Count];
				for (int i = 0; i < args.Length; i++)
					args [i] = genInst.GenericArguments [i].CopyGeneric (newBasis, oldBasis);
				
				return genInst.ElementType.MakeGeneric (args);
			}
			
			return type;
		}
	*/		
		public static TypeReference CopyGeneric (this TypeReference type, IGenericParameterProvider newBasis, IDictionary<IGenericParameterProvider,int> skewSet)
		{
			if (!newBasis.HasGenericParameters)
				return type;
					
			int skew;
			var genParam = type as GenericParameter;
			if (genParam != null && skewSet.TryGetValue (genParam.Owner, out skew)) {
				return newBasis.GenericParameters [skew+genParam.Position];
			}
			
			var genInst = type as GenericInstanceType;
			if (genInst != null) {
				
				var args = new TypeReference [genInst.GenericArguments.Count];
				for (int i = 0; i < args.Length; i++)
					args [i] = genInst.GenericArguments [i].CopyGeneric (newBasis, skewSet);
				
				return genInst.ElementType.MakeGeneric (args);
			}
			
			return type;
		}
		
		public static MethodReference CopyGeneric (this MethodReference method, ModuleDefinition module, IGenericParameterProvider newBasis, IDictionary<IGenericParameterProvider,int> skewSet)
		{
			var fixedMethod = method;
			
			if (newBasis.HasGenericParameters) {
			
				var genInst = method as GenericInstanceMethod;
				if (genInst != null) {
				
					var args = new TypeReference [genInst.GenericArguments.Count];
					for (int i = 0; i < genInst.GenericArguments.Count; i++)
						args [i] = genInst.GenericArguments [i].CopyGeneric (newBasis, skewSet);
					
					fixedMethod = genInst.ElementMethod.MakeGeneric (args);
					
				} else if (method.DeclaringType is GenericInstanceType)
					fixedMethod = method.MakeGeneric (method.DeclaringType.CopyGeneric (newBasis, skewSet));
			}
			/*
			foreach (var param in fixedMethod.Parameters) {
				var paramType = param.ParameterType.CopyGeneric (newBasis, skewSet);
				param.ParameterType = module.Import (paramType, fixedMethod);
			}
			*/
			
			return fixedMethod;
		}
		
		public static bool IsBuiltInFutureType (this TypeReference type)
		{
			if (type == null)
				return false;
			
			var typeName = type.FullName;
			return typeName == "Cirrus.Future" || typeName.StartsWith ("Cirrus.Future`1");
		}
		public static bool IsBuiltInFutureTypeOrVoid (this TypeReference type)
		{
			return type.IsVoid () || type.IsBuiltInFutureType ();
		}
		
		public static bool IsFutureType (this TypeReference type)
		{
			if (type == null)
				return false;
			
			if (IsBuiltInFutureType (type))
				return true;
			
			return IsFutureType (type.Resolve ().BaseType);
		}
		public static bool IsVoid (this TypeReference type)
		{
			return type.FullName == "System.Void";
		}
		
		// Returns the first instruction in the chain of instructions ultimately responsible for the last item that
		//  will be on the stack when the specified instruction is executed.
		// This effectively allows you to insert instructions to push items stack penultimate
		public static IEnumerable<Instruction> FindLastStackItem (this ControlFlowGraph cfg, InstructionBlock block, Instruction current)
		{
			return cfg.FindLastStackItem (block, current, i => false);	
		}
		
		// Finds the first instruction(s) responsible for the item at the top of the stack as visible from the specified instruction.
		// isBarrier allows you to specify Instructions that you cannot preceed in the stack walk (i.e. continuations :) 
		public static IEnumerable<Instruction> FindLastStackItem (this ControlFlowGraph cfg, InstructionBlock block, Instruction current, Func<Instruction, bool> isBarrier)
		{
			return cfg.FindStackHeight (block, current, cfg.GetData (current).StackBefore - 1, isBarrier);
		}
		
		// Finds the last instruction(s) at the specified stack height as visible from the specified instruction
		public static IEnumerable<Instruction> FindStackHeight (this ControlFlowGraph cfg, InstructionBlock block, Instruction current, int stackHeight)
		{
			return cfg.FindStackHeight (block, current, stackHeight, i => false);	
		}
		
		// isBarrier allows you to specify Instructions that you cannot preceed in the stack walk (i.e. continuations :) 
		public static IEnumerable<Instruction> FindStackHeight (this ControlFlowGraph cfg, InstructionBlock block, Instruction current, int stackHeight, Func<Instruction, bool> isBarrier)
		{
			
			while (cfg.GetData (current).StackBefore > stackHeight) {
				if (current == block.First) {
					return cfg.GetPredecessors (block).Select (b => cfg.FindStackHeight (b, b.Last, stackHeight, isBarrier)).Flatten ().Distinct ();
				} else
					current = current.Previous;
				
				if (isBarrier (current))
					break;
			}
			
			if (current.OpCode == OpCodes.Dup)
				return cfg.FindLastStackItem (block, current, isBarrier);
			
			return new Instruction [] { current };
		}
		
		public static IEnumerable<InstructionBlock> GetPredecessors (this ControlFlowGraph cfg, InstructionBlock ib)
		{
			return cfg.Blocks.Where (b => b.Successors.Contains (ib));
		}
			
		public static void ComputeNewStackHeight (this MethodReference method, Instruction instruction, ref int currentStackHeight)
		{
			currentStackHeight = currentStackHeight + GetPushDelta (instruction) - GetPopDelta (currentStackHeight, method, instruction);
		}

		public static int GetPushDelta (Instruction instruction)
		{
			OpCode code = instruction.OpCode;
			switch (code.StackBehaviourPush) {
			case StackBehaviour.Push0:
				return 0;

			case StackBehaviour.Push1:
			case StackBehaviour.Pushi:
			case StackBehaviour.Pushi8:
			case StackBehaviour.Pushr4:
			case StackBehaviour.Pushr8:
			case StackBehaviour.Pushref:
				return 1;

			case StackBehaviour.Push1_push1:
				return 2;

			case StackBehaviour.Varpush:
				if (code.FlowControl == FlowControl.Call) {
					var method = (IMethodSignature) instruction.Operand;
					return method.ReturnType.IsVoid ()? 0 : 1;
				}

				break;
			}
			throw new ArgumentException (instruction.ToString ());
		}

		public static int GetPopDelta (int stackHeight, MethodReference method, Instruction instruction)
		{
			OpCode code = instruction.OpCode;
			switch (code.StackBehaviourPop) {
			case StackBehaviour.Pop0:
				return 0;
			case StackBehaviour.Popi:
			case StackBehaviour.Popref:
			case StackBehaviour.Pop1:
				return 1;

			case StackBehaviour.Pop1_pop1:
			case StackBehaviour.Popi_pop1:
			case StackBehaviour.Popi_popi:
			case StackBehaviour.Popi_popi8:
			case StackBehaviour.Popi_popr4:
			case StackBehaviour.Popi_popr8:
			case StackBehaviour.Popref_pop1:
			case StackBehaviour.Popref_popi:
				return 2;

			case StackBehaviour.Popi_popi_popi:
			case StackBehaviour.Popref_popi_popi:
			case StackBehaviour.Popref_popi_popi8:
			case StackBehaviour.Popref_popi_popr4:
			case StackBehaviour.Popref_popi_popr8:
			case StackBehaviour.Popref_popi_popref:
				return 3;

			case StackBehaviour.PopAll:
				return stackHeight;

			case StackBehaviour.Varpop:
				if (code.FlowControl == FlowControl.Call) {
					var callee = (IMethodSignature) instruction.Operand;
					int count = callee.Parameters.Count;
					if (callee.HasThis && OpCodes.Newobj.Value != code.Value)
						++count;

					return count;
				}

				if (code.Code == Code.Ret)
					return  method.ReturnType.IsVoid ()? 0 : 1;

				break;
			}
			throw new ArgumentException (instruction.ToString ());
		}
		
		public static bool IsJumpTarget (this Mono.Cecil.Cil.MethodBody body, Instruction inst)
		{
			if (inst == null)
				throw new ArgumentNullException ();
			
			var jumps = from i in body.Instructions
						where i.Operand == inst || (i.Operand is Instruction [] && ((Instruction [])i.Operand).Contains (inst))
						select i;
			
			return jumps.Any ();
		}
		
		public static void RedirectJumps (this IEnumerable<Instruction> insts, Instruction oldTarget, Instruction newTarget)
		{
			if (oldTarget == null)
					return;
			if (newTarget == null)
					throw new ArgumentNullException ("newTarget");
				
			var jumps = from i in insts
						where i.Operand == oldTarget || i.Operand is Instruction []
						select i;
				
			foreach (var inst in jumps) {
				var ops = inst.Operand as Instruction [];
				if (ops != null) {
					for (int i = 0; i < ops.Length; i++)
						if (ops [i] == oldTarget) ops [i] = newTarget;
					inst.Operand = ops;
				} else
					inst.Operand = newTarget;
			}
		}
		
		// From Cecil rocks (with modifications):
		
		public static void ComputeOffsetsAndMaxStack (this Mono.Cecil.Cil.MethodBody body)
		{
			var offset = 0;
			var stackHeight = 0;
			var maxStack = 0;
			
			for (int i = 0; i < body.Instructions.Count; i++) {
				var instruction = body.Instructions [i];
				
				ComputeNewStackHeight (body.Method, instruction, ref stackHeight);
				maxStack = Math.Max (maxStack, stackHeight);
				
				instruction.Offset = offset;
				offset += instruction.GetSize ();     
			}
			
			body.MaxStackSize = maxStack;
		}
		
		public static void SimplifyMacros (this Mono.Cecil.Cil.MethodBody self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			foreach (var instruction in self.Instructions) {
				if (instruction.OpCode.OpCodeType != OpCodeType.Macro)
					continue;

				switch (instruction.OpCode.Code) {
				
				case Code.Ldc_I4_S:
					ExpandMacro (instruction, OpCodes.Ldc_I4, (int) (sbyte) instruction.Operand);
					break;
				case Code.Br_S:
					instruction.OpCode = OpCodes.Br;
					break;
				case Code.Brfalse_S:
					instruction.OpCode = OpCodes.Brfalse;
					break;
				case Code.Brtrue_S:
					instruction.OpCode = OpCodes.Brtrue;
					break;
				case Code.Beq_S:
					instruction.OpCode = OpCodes.Beq;
					break;
				case Code.Bge_S:
					instruction.OpCode = OpCodes.Bge;
					break;
				case Code.Bgt_S:
					instruction.OpCode = OpCodes.Bgt;
					break;
				case Code.Ble_S:
					instruction.OpCode = OpCodes.Ble;
					break;
				case Code.Blt_S:
					instruction.OpCode = OpCodes.Blt;
					break;
				case Code.Bne_Un_S:
					instruction.OpCode = OpCodes.Bne_Un;
					break;
				case Code.Bge_Un_S:
					instruction.OpCode = OpCodes.Bge_Un;
					break;
				case Code.Bgt_Un_S:
					instruction.OpCode = OpCodes.Bgt_Un;
					break;
				case Code.Ble_Un_S:
					instruction.OpCode = OpCodes.Ble_Un;
					break;
				case Code.Blt_Un_S:
					instruction.OpCode = OpCodes.Blt_Un;
					break;
				case Code.Leave_S:
					instruction.OpCode = OpCodes.Leave;
					break;
				}
			}
		}

		static void ExpandMacro (Instruction instruction, OpCode opcode, object operand)
		{
			instruction.OpCode = opcode;
			instruction.Operand = operand;
		}

		static void MakeMacro (Instruction instruction, OpCode opcode)
		{
			instruction.OpCode = opcode;
			instruction.Operand = null;
		}

		public static void OptimizeMacros (this Mono.Cecil.Cil.MethodBody self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			var method = self.Method;

			foreach (var instruction in self.Instructions) {
				int index;
				switch (instruction.OpCode.Code) {
				case Code.Ldarg:
					index = ((ParameterDefinition) instruction.Operand).Index;
					if (index == -1 && instruction.Operand == self.ThisParameter)
						index = 0;
					else if (method.HasThis)
						index++;

					switch (index) {
					case 0:
						MakeMacro (instruction, OpCodes.Ldarg_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Ldarg_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Ldarg_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Ldarg_3);
						break;
					default:
						if (index < 256)
							ExpandMacro (instruction, OpCodes.Ldarg_S, instruction.Operand);
						break;
					}
					break;
				case Code.Ldloc:
					index = ((VariableDefinition) instruction.Operand).Index;
					switch (index) {
					case 0:
						MakeMacro (instruction, OpCodes.Ldloc_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Ldloc_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Ldloc_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Ldloc_3);
						break;
					default:
						if (index < 256)
							ExpandMacro (instruction, OpCodes.Ldloc_S, instruction.Operand);
						break;
					}
					break;
				case Code.Stloc:
					index = ((VariableDefinition) instruction.Operand).Index;
					switch (index) {
					case 0:
						MakeMacro (instruction, OpCodes.Stloc_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Stloc_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Stloc_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Stloc_3);
						break;
					default:
						if (index < 256)
							ExpandMacro (instruction, OpCodes.Stloc_S, instruction.Operand);
						break;
					}
					break;
				case Code.Ldarga:
					index = ((ParameterDefinition) instruction.Operand).Index;
					if (index == -1 && instruction.Operand == self.ThisParameter)
						index = 0;
					else if (method.HasThis)
						index++;
					if (index < 256)
						ExpandMacro (instruction, OpCodes.Ldarga_S, instruction.Operand);
					break;
				case Code.Ldloca:
					if (((VariableDefinition) instruction.Operand).Index < 256)
						ExpandMacro (instruction, OpCodes.Ldloca_S, instruction.Operand);
					break;
				case Code.Ldc_I4:
					int i = (int) instruction.Operand;
					switch (i) {
					case -1:
						MakeMacro (instruction, OpCodes.Ldc_I4_M1);
						break;
					case 0:
						MakeMacro (instruction, OpCodes.Ldc_I4_0);
						break;
					case 1:
						MakeMacro (instruction, OpCodes.Ldc_I4_1);
						break;
					case 2:
						MakeMacro (instruction, OpCodes.Ldc_I4_2);
						break;
					case 3:
						MakeMacro (instruction, OpCodes.Ldc_I4_3);
						break;
					case 4:
						MakeMacro (instruction, OpCodes.Ldc_I4_4);
						break;
					case 5:
						MakeMacro (instruction, OpCodes.Ldc_I4_5);
						break;
					case 6:
						MakeMacro (instruction, OpCodes.Ldc_I4_6);
						break;
					case 7:
						MakeMacro (instruction, OpCodes.Ldc_I4_7);
						break;
					case 8:
						MakeMacro (instruction, OpCodes.Ldc_I4_8);
						break;
					default:
						if (i >= -128 && i < 128)
							ExpandMacro (instruction, OpCodes.Ldc_I4_S, (sbyte) i);
						break;
					}
					break;
				}
			}

			OptimizeBranches (self);
		}

		static void OptimizeBranches (Mono.Cecil.Cil.MethodBody body)
		{
			foreach (var instruction in body.Instructions) {
				if (instruction.OpCode.OperandType != OperandType.InlineBrTarget)
					continue;

				if (OptimizeBranch (instruction))
					ComputeOffsetsAndMaxStack (body);
			}
		}

		static bool OptimizeBranch (Instruction instruction)
		{
			var offset = ((Instruction) instruction.Operand).Offset - (instruction.Offset + instruction.OpCode.Size + 4);
			if (!(offset >= -128 && offset <= 127))
				return false;

			switch (instruction.OpCode.Code) {
			case Code.Br:
				instruction.OpCode = OpCodes.Br_S;
				break;
			case Code.Brfalse:
				instruction.OpCode = OpCodes.Brfalse_S;
				break;
			case Code.Brtrue:
				instruction.OpCode = OpCodes.Brtrue_S;
				break;
			case Code.Beq:
				instruction.OpCode = OpCodes.Beq_S;
				break;
			case Code.Bge:
				instruction.OpCode = OpCodes.Bge_S;
				break;
			case Code.Bgt:
				instruction.OpCode = OpCodes.Bgt_S;
				break;
			case Code.Ble:
				instruction.OpCode = OpCodes.Ble_S;
				break;
			case Code.Blt:
				instruction.OpCode = OpCodes.Blt_S;
				break;
			case Code.Bne_Un:
				instruction.OpCode = OpCodes.Bne_Un_S;
				break;
			case Code.Bge_Un:
				instruction.OpCode = OpCodes.Bge_Un_S;
				break;
			case Code.Bgt_Un:
				instruction.OpCode = OpCodes.Bgt_Un_S;
				break;
			case Code.Ble_Un:
				instruction.OpCode = OpCodes.Ble_Un_S;
				break;
			case Code.Blt_Un:
				instruction.OpCode = OpCodes.Blt_Un_S;
				break;
			case Code.Leave:
				instruction.OpCode = OpCodes.Leave_S;
				break;
			}

			return true;
		}

		
		// General utility:
		
		public static IEnumerable<T> Flatten<T> (this IEnumerable<IEnumerable<T>> enumerable)
		{
			foreach (var subenumerable in enumerable)
				foreach (var item in subenumerable)
					yield return item;
		}
		
		public static T LastOrDefault<T> (this IEnumerable<T> enumerable, Func<T, T, bool> condition, T initial)
		{
			var lastItem = initial;
			
			foreach (var item in enumerable) {
			
				if (condition (lastItem, item))
					lastItem = item;
			}
			
			return lastItem;
		}
	}
}

