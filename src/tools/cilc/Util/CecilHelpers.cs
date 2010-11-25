/*
	CecilHelpers.cs: Static and extension utility methods for Mono.Cecil and Cecil.Decompiler
  
	Copyright (c) 2010 Alexander Corrado
	
	Methods ComputeNewStackHeight, GetPushDelta, GetPopDelta:
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
	
	public delegate Instruction StepChecker (Instruction currentInstruction, Instruction nextPreviousInstruction);
	
	public static class CecilHelpers {
		
		
		public static TypeReference MakeGeneric (this TypeReference type, params TypeReference [] args)
		{
			if (type.GenericParameters.Count != args.Length)
				throw new ArgumentException ("Invalid number of generic type arguments supplied");
			
			var genericTypeRef = new GenericInstanceType (type);
			foreach (var arg in args)
				genericTypeRef.GenericArguments.Add (arg);
			
			return genericTypeRef;
		}
		
		// Returns the first instruction in the chain of instructions ultimately responsible for the last item that
		//  will be on the stack when the specified instruction is executed.
		// This effectively allows you to insert instructions to push items stack penultimate
		public static IEnumerable<Instruction> FindLastStackItem (this ControlFlowGraph cfg, InstructionBlock block, Instruction current)
		{
			return cfg.FindLastStackItem (block, current, i => false);	
		}
		
		// isBarrier allows you to specify Instructions that you cannot preceed in the stack walk (i.e. continuations :) 
		public static IEnumerable<Instruction> FindLastStackItem (this ControlFlowGraph cfg, InstructionBlock block, Instruction current, Func<Instruction, bool> isBarrier)
		{
			return cfg.FindStackHeight (block, current, cfg.GetData (current).StackBefore - 1, isBarrier);
		}
		
		// Finds the most recent instructions (relative to passed instruction) before which the stack height is as specified
		public static IEnumerable<Instruction> FindStackHeight (this ControlFlowGraph cfg, InstructionBlock block, Instruction current, int stackHeight)
		{
			return cfg.FindStackHeight (block, current, stackHeight, i => false);	
		}
		
		// isBarrier allows you to specify Instructions that you cannot preceed in the stack walk (i.e. continuations :) 
		public static IEnumerable<Instruction> FindStackHeight (this ControlFlowGraph cfg, InstructionBlock block, Instruction current, int stackHeight, Func<Instruction, bool> isBarrier)
		{
			
			while (cfg.GetData (current).StackBefore > stackHeight) {
				if (current == block.First) {
					return cfg.GetPredecessors (block).Select (b => cfg.FindStackHeight (b, b.Last, stackHeight, isBarrier)).Collapse ().Distinct ();
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
					return method.ReturnType.FullName == "System.Void"? 0 : 1;
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
					return  method.ReturnType.FullName == "System.Void"? 0 : 1;

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
		
		public static void RedirectJumps (this Mono.Cecil.Cil.MethodBody body, Instruction oldTarget, Instruction newTarget)
		{
			if (oldTarget == null)
					return;
			if (newTarget == null)
					throw new ArgumentNullException ("newTarget");
				
			var jumps = from i in body.Instructions
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
		
		// returns max stack
		public static int ComputeOffsetsAndMaxStack (this Mono.Cecil.Cil.MethodBody body)
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
			
			return maxStack;
		}
		
		public static IEnumerable<T> Collapse<T> (this IEnumerable<IEnumerable<T>> enumerable)
		{
			foreach (var subenumerable in enumerable)
				foreach (var item in subenumerable)
					yield return item;
		}
	}
}

