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
		
		//public static int width = 0;
		//public static int height = 0;
		//public static string title = null;
		
		public static OptionSet Options = new OptionSet () {
			
			{ "h|?|help", "Show this message", v => { ShowHelp (Options); Environment.Exit (1); } },
			{ "t=|target=", "Specifies the target of the output:\n" + 
								"\tIL - output to an assembly of the same type as the input",
				(string v) => {	target = targets [v.ToLower ()];	}
			},
			{ "o=|output=", "Specifies the name of the output (type depends on target)", v => { output = v; } }/*,
			{ "w=|width=", "Specifies the width of the main window", v => { width = int.Parse (v); } },
			{ "h=|height=", "Specifies the height of the main window", v => { height = int.Parse (v); } },
			{ "m=|maintitle=", "Specifies the title of the main window", v => { title = v; } }*/
		};
		
		public static int Main (string[] args)
		{
			List<string> inputFiles;
			
			try {
				inputFiles = Options.Parse (args);
			} catch (OptionException) {
				System.Console.Error.WriteLine ("Try `{0} --help' for more information.", Path.GetFileNameWithoutExtension (Environment.GetCommandLineArgs () [0]));
				return 1;
			}
			
			if (inputFiles == null || inputFiles.Count == 0) {
				System.Console.Error.WriteLine ("No input files specified.\nTry `{0} --help' for more information.", Path.GetFileNameWithoutExtension (Environment.GetCommandLineArgs () [0]));
				return 1;
			}
				
			target.ProcessFiles (inputFiles);
			//target.CreatePlatformExecutible ();
			return 0;
		}
		
		public static void ShowHelp (OptionSet opts)
		{
			System.Console.Error.WriteLine ("Cirrus IL post-Compiler (CILC), v0.1\nCopyright 2010 Alex Corrado.\n");
			System.Console.Error.WriteLine ("Usage: {0} [options] assembly1 assembly2 ... assemblyN\n", Path.GetFileNameWithoutExtension (Environment.GetCommandLineArgs () [0]));
			opts.WriteOptionDescriptions (System.Console.Error);
		}
	}
}

