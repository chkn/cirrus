using System;
using System.IO;
using System.Linq;

using Mono.Cecil;

using Cecil.Decompiler.Languages;
using Cirrus.Tools.Cilc.Targets.Web;

namespace Cirrus.Tools.Cilc.Targets {
	
	public abstract class DecompileTarget : Target {
		
		public Stream Stream { get; protected set; }
		
		private string defaultExt;
		
		public DecompileTarget (string defaultExtension)
		{
			Stream = new MemoryStream ();
			
			if (!defaultExtension.StartsWith ("."))
				defaultExt = "." + defaultExtension;
			else
				defaultExt = defaultExtension;
		}
		
		public override void SaveOutput (ModuleDefinition module, string inputFileName)
		{
			using (var file = File.Create (OutputName ?? (Path.GetFileNameWithoutExtension (inputFileName) + defaultExt))) {
				((MemoryStream)Stream).WriteTo (file);
				file.Flush ();
			}
		}
	}
}

