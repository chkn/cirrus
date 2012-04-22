/*
	Main.cs: Entry point for the Cirrus IL post-Compiler (CILC)
  
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
using System.IO;
using System.Collections.Generic;
using NDesk.Options;

using Cirrus.Tools.Cilc.Targets;

namespace Cirrus.Tools.Cilc {
	static class MainClass {
		
		public static readonly Dictionary<string,Target> targets = new Dictionary<string, Target> () {
			{ "il", new TargetIL () }
		};

		public static Target target = targets ["il"];
		public static string output = null;
		public static bool debug = false;
		
		public static List<string> references = new List<string> ();
		public static string coreLib;
		
		public static OptionSet Options = new OptionSet ()
		{	
			{ "h|?|help", "Show this message", v => { ShowHelp (Options); Environment.Exit (1); } },
			{ "g|debug", "Generate debug-friendly output", v => debug = true },
			{ "t=|target=", "Specifies the target of the output:\n" + 
								"\t\tIL - output to an assembly of the same type as the input\n",
				v => { target = targets [v.ToLower ()];	}
			},
			{ "out=", "Specifies the name of the output (type depends on target)", v => { output = v; } },
			{ "reference=|r=", "Additional reference assembly (Cirrus libraries and corlib are added by default)",
				v => { references.Add (v); }
			},
			{ "core=|c=", "Specify the location of the Cirrus.Core assembly (by default, current directory)",
				v => { coreLib = v; }
			}
		};

		public static int Main (string[] args)
		{
			List<string> inputFiles;
			
			try {
				inputFiles = Options.Parse (args);
			} catch (OptionException e) {
				Console.Error.WriteLine (e.Message);
				Console.Error.WriteLine ("Try `{0} --help' for more information.", Path.GetFileNameWithoutExtension (Environment.GetCommandLineArgs () [0]));
				return 1;
			}
			
			if (inputFiles == null || inputFiles.Count == 0) {
				Console.Error.WriteLine ("No input files specified.\nTry `{0} --help' for more information.", Path.GetFileNameWithoutExtension (Environment.GetCommandLineArgs () [0]));
				return 1;
			}
			
			if (coreLib == null)
				coreLib = Path.Combine (Environment.CurrentDirectory, "Cirrus.Core.dll");
			if (!File.Exists (coreLib)) {
				Console.Error.WriteLine ("Cirrus.Core.dll not found in current directory. Set the path with --core option.");
				return 1;
			}

			target.OutputName = output;
			target.Debug = debug;
			target.References = references;
			target.CoreAssembly = coreLib;
			target.ProcessFiles (inputFiles);
			return 0;
		}
		
		public static void ShowHelp (OptionSet opts)
		{
			Console.Error.WriteLine ("cilc: Cirrus IL post-Compiler, v0.2\nCopyright 2010 - 2012 Alex Corrado\n");
			Console.Error.WriteLine ("Usage: {0} [options] assembly1 assembly2 ... assemblyN\n", Path.GetFileNameWithoutExtension (Environment.GetCommandLineArgs () [0]));
			opts.WriteOptionDescriptions (System.Console.Error);
		}
	}
}

