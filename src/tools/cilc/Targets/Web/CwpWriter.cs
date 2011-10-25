using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Mono.Cecil;

using Cecil.Decompiler;
using Cecil.Decompiler.Ast;
using Cecil.Decompiler.Languages;

namespace Cirrus.Tools.Cilc.Targets.Web {
	public class CwpWriter : ILanguageWriter {
		
		public CwpWriter (DecompilePipeline pipeline, Stream stream)
		{
		}
		
		public void Write (TypeDefinition type)
		{
			throw new NotImplementedException ();
		}

		public void Write (MethodDefinition method)
		{
			throw new NotImplementedException ();
		}
		
		public void Write (Expression expression)
		{
			throw new NotImplementedException ();
		}
		
		public void Write (Statement statement)
		{
			throw new NotImplementedException ();
		}
		
	}
}

