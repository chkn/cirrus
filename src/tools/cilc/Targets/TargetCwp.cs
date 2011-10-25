using System;
using System.IO;

using Mono.Cecil;

using Cecil.Decompiler.Languages;
using Cirrus.Tools.Cilc.Targets.Web;

namespace Cirrus.Tools.Cilc.Targets {
	
	public class TargetCwp : DecompileTarget {
		
		public CwpWriter Writer { get; protected set; }
		
		public TargetCwp () : base (".cwp")
		{
			var pipeline = new DecompilePipeline ();
			Writer = new CwpWriter (pipeline, Stream);
		}
		
		public override bool ProcessType (TypeDefinition type)
		{
			if (base.ProcessType (type)) {
				Writer.Write (type);
				return true;
			}
			
			return false;
		}
		
	}
}

