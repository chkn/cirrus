/*
	Target.cs: An abstract postcompiler target
  
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
using System.Collections.Generic;

using Mono.Cecil;

namespace Cirrus.Tools.Cilc {
	public abstract class Target {
		
		public string MainAssembly { get; set; }
		public string EntryPoint { get; set; }
		public string OutputName { get; set; }
		public IList<string> References { get; set; }
		public bool Debug { get; set; }

		public string CoreAssembly {
			set { core = AssemblyDefinition.ReadAssembly (value); }
		}

		protected AssemblyDefinition core;

		public virtual void ProcessFiles (IEnumerable<string> files)
		{
			foreach (var file in files) {
				var module = ModuleDefinition.ReadModule (file, new ReaderParameters { ReadSymbols = Debug });
				if (ProcessModule (module))
					SaveOutput (module, file);
			}
		}
		
		// Returns true if module was modified
		public virtual bool ProcessModule (ModuleDefinition module)
		{
			if (!module.HasTypes)
				return false;
			
			bool modified = false;
			foreach (var type in module.Types)
				modified |= ProcessType (type);
			
			return modified;
		}
		
		// Returns true if type was modified
		public virtual bool ProcessType (TypeDefinition type)
		{
			if (!type.HasMethods)
				return false;
			
			bool modified = false;
			foreach (var method in type.Methods)
				modified |= ProcessMethod (method);
			
			foreach (var nested in type.NestedTypes)
				modified |= ProcessType (nested);
			
			return modified;
		}
		
		// Returns true if method was modified
		public virtual bool ProcessMethod (MethodDefinition method)
		{
			if (method.CustomAttributes.Any (a => a.AttributeType.FullName == "Cirrus.EntryPointAttribute")) {
				var potentialEntryPoint = method.DeclaringType.FullName + "::" + method.Name;
				
				if (EntryPoint != null)
					throw new Error (method.Module.Name, "found entry point " + potentialEntryPoint + ", but already had " + EntryPoint);
				
				EntryPoint = potentialEntryPoint;
				MainAssembly = method.Module.Name;
			}
			return false;
		}
		
		
		public abstract void SaveOutput (ModuleDefinition module, string inputFileName);
					                 
		public class Error : Exception {
			public string File { get; private set; }
			
			public Error (string file, string message) : base (message)
			{
				this.File = file;
			}
			
			public override string ToString ()
			{
				return string.Format ("Error: In {0}, {1}", File, Message);
			}
		}
	}
}

