/*
	Platform.cs: Cirrus's best PAL :)
  
	Copyright (c) 2010-2011 Alexander Corrado
  
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
using System.Linq;
using System.Collections.Generic;

using System.Reflection;
using Cirrus.UI;

namespace Cirrus {
	
	public partial class Platform {
		// (the rest of this class is in DispatchThread.cs)
		
		private static Platform current;
		
		public static Platform Current {
			get {
				return current;	
			}
		}
		
		protected MethodBase EntryPoint { get; set; }
		
		// UI -related properties follow:
		
		protected string Title { get; set; }
		protected int Width { get; set; }
		protected int Height { get; set; }
		
		public virtual RootWidget Root { get; protected set; }
		
		// -----
		
		/// <summary>
		/// Initializes a new headless Platform.
		/// </summary>
		/// <description>
		/// A headless platform does not have any GUI, so the UI-related properties will
		/// not be set.
		/// </description>
		public Platform ()
		{
			current = this;	
		}
		
		protected virtual void PrintUsage ()
		{
			var basename = Path.GetFileNameWithoutExtension (Environment.GetCommandLineArgs ()[0]);
			Console.Error.WriteLine ("Usage: {0} [options] assembly", basename);
			Console.Error.WriteLine ("\tOptions (all optional):");
			Console.Error.WriteLine ("\t\t--title \"Application Title\" - Set the titlebar of the main window.");
			Console.Error.WriteLine ("\t\t--width 123 - Set the width of the main window.");
			Console.Error.WriteLine ("\t\t--height 123 - Set the height of the main window.");
			Environment.Exit (1);
		}
		
		protected Platform (IEnumerable<string> args)
			: this (args.LastOrDefault ())
		{
			var arg = args.GetEnumerator ();
			while (arg.MoveNext ()) {
			
				switch (arg.Current) {
				
				case "--title": if (arg.MoveNext ()) Title = ReadStringArg (arg); break;
				case "--width": if (arg.MoveNext ()) Width = int.Parse (arg.Current); break;
				case "--height": if (arg.MoveNext ()) Height = int.Parse (arg.Current); break;
				
				}
			}
		}
		
		protected string ReadStringArg (IEnumerator<string> arg)
		{
			var current = arg.Current;
			if (current [0] != '"')
				return current;
			
			current = current.Substring (1);
			while (arg.MoveNext ()) {
			
				current += arg.Current;
				
				if (current.EndsWith ("\"")) {
					current = current.TrimEnd ('"');
					break;
				}
			}
			return current;
		}
		
		protected Platform (string userAssembly) : this ()
		{
			if (userAssembly == null)
				PrintUsage ();
			
			// set defaults:
			Title = "Cirrus Application";
			Width = 300;
			Height = 300;
			
			var assembly = Assembly.LoadFrom (userAssembly);
			
			EntryPoint = (from t in assembly.GetTypes ()
						  from m in t.GetMethods ()
						  where m.IsStatic && !m.GetParameters ().Any () && m.IsDefined (typeof (EntryPointAttribute), false)
						  select m).SingleOrDefault ();
			
			if (EntryPoint == null)
				throw new EntryPointNotFoundException (userAssembly);
		}
	}
}

