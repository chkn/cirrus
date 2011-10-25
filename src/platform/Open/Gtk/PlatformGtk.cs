/*
	Bootstrap.cs: Cirrus Client Bootstrapper
  
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
using System.Runtime.InteropServices;
using Gtk;

using Cirrus;

namespace Cirrus.Open {
	
	public class PlatformGtk : Platform {
		public static void Main (string[] args)
		{
			new PlatformGtk (args);
		}
		
		public PlatformGtk (string [] args) : base (args)
		{
			Application.Init ();
			
			var win = new Gtk.Window (Title);
			win.DeleteEvent += delegate {
				Application.Quit ();
			};
			win.SetSizeRequest (Width, Height);
			
			var canvas = new GtkCanvas (Width, Height);
			win.Add (canvas);
			
			Root = new GtkRoot (win, canvas);

			var dispatch = new DispatchThread (Thread.Current);
			dispatch.OnDispatch += () => Gtk.Application.Invoke (delegate { Thread.Current.RunSingleIteration (); });
			
			win.ShowAll ();
			
			EntryPoint.Invoke (null, null);
			dispatch.Run ();
			
			Application.Run ();
			dispatch.Quit ();
		}
		
		
		
	}
}
