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

// NOTE: This will eventually be replaced with a better solution (Direct2D perhaps?)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Cirrus;

namespace Cirrus.Silverlight {
	
    public class PlatformWpf : Platform {

        [STAThread]
        public static void Main (string[] args)
        {
            new PlatformWin ();
        }

        public PlatformWpf () : base(null, true)
        {
            var app = new Application ();
            var slCanvas = new Canvas ();
            var win = new Window
            {
                Title = Title,
                Width = Width,
                Height = Height,
                Content = slCanvas
            };

            var cirrusCanvas = new CirrusCanvas(slCanvas, Width, Height);
            MainCanvas = cirrusCanvas;

            win.Show ();
			
            EntryPoint.Invoke (null, null);

            var timer = new DispatcherTimer ();
            timer.Tick += runDelegate;
            timer.Interval = TimeSpan.FromMilliseconds (1);

            timer.Start ();
            app.Run ();
        }
		
		private static void RunIteration (object sender, EventArgs e)
        {
            TimeSpan sleepTime;
            var timer = sender as DispatcherTimer;

            if (Thread.Current.RunSingleIteration (out sleepTime))
                timer.Interval = sleepTime;
            else
                timer.Stop ();
        }
        private static EventHandler runDelegate = new EventHandler(RunIteration);
    }
}
