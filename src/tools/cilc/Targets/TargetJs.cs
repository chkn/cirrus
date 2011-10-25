using System;
using System.IO;

using Mono.Cecil;

using Cecil.Decompiler.Languages;
using Cirrus.Tools.Cilc.Targets.Web;

namespace Cirrus.Tools.Cilc.Targets {
	
	public class TargetJs : DecompileTarget {
		
		public JsWriter Writer { get; protected set; }
		
		public TargetJs () : base (".js")
		{
			var pipeline = new DecompilePipeline ();
			Writer = new JsWriter (pipeline, Stream);
		}
		
		public override bool ProcessType (TypeDefinition type)
		{
			Writer.Write (type);
			return true;
		}

	}
}

