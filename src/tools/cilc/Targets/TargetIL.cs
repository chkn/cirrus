/*
	TargetIL.cs: CIL-to-CIL post-compile target
   
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecil.Decompiler.Cil;

using Cirrus.Tools.Cilc.Targets.IL;
using Cirrus.Tools.Cilc.Util;

namespace Cirrus.Tools.Cilc.Targets {
	
	public class TargetIL : Target {
		
		// Returns true if method was modified
		public override bool ProcessMethod (MethodDefinition method)
		{	
			base.ProcessMethod (method);
			if (!method.HasBody)
				return false;
			
			// Look for one of the 2 IL trigger patterns:
			//  1) Call Future (or subclass) Wait ()
			//  2) Call Thread.Yield
			
			bool transformed = false;
			var returnType = method.ReturnType;
			
			foreach (var instruction in method.Body.Instructions) {
				if (instruction.OpCode.FlowControl != FlowControl.Call)
					continue;
				
				var callee = instruction.Operand as MethodReference;
				
				//1
				if (callee.Name == "Wait" && callee.DeclaringType.IsFutureType ()) {
					if (!returnType.IsBuiltInFutureTypeOrVoid ())
						throw new Error (method.Module.Name,
						                 string.Format ("method `{0}' in type `{1}' uses asynchronous Wait, but its return type is not void, Cirrus.Future, or Cirrus.Future<T>",
						                                method.Name, method.DeclaringType.Name));
					
					TransformAsyncMethod (method);
					transformed = true;
					break;
				}
				
				//2
				if (callee.Name == "Yield" && callee.DeclaringType.FullName == "Cirrus.Thread") {
					if (!returnType.IsBuiltInFutureTypeOrVoid ())
						throw new Error (method.Module.Name,
						                 string.Format ("method `{0}' in type `{1}' uses Thread.Yield, but its return type is not void, Cirrus.Future, or Cirrus.Future<T>",
						                                method.Name, method.DeclaringType.Name));

					TransformAsyncMethod (method);
					transformed = true;
					break;
				}
			}
			
			return transformed;
		}
		
		public override void SaveOutput (ModuleDefinition module, string inputFileName)
		{
			module.Write (OutputName ?? inputFileName, new WriterParameters { WriteSymbols = Debug });
		}
		
		protected virtual void TransformAsyncMethod (MethodDefinition method)
		{
			if (method.Parameters.Any (p => p.IsOut))
				throw new Error (method.Module.Name,
				                 string.Format ("method `{0}' in type `{1}' cannot be transformed into an asynchronous coroutine becuase it has a ref or out parameter",
				                                method.Name, method.DeclaringType.Name)); 
			
			// Create Future implementation...
			method.Body.SimplifyMacros ();
			AsyncMethodTransform.Transform (method, core, Debug);
			method.Body.OptimizeMacros ();
		}
		
	}
}

