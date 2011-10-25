/*
	DispatchThread.cs: Helper for integrating Cirrus run loop with platform GUI run loop
  
	Copyright (c) 2011 Alexander Corrado
  
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

namespace Cirrus {
	
	public partial class Platform {
		
		protected sealed class DispatchThread {
	
			public event Action OnDispatch;
			public bool ShouldRun { get; set; }
			
			private Cirrus.Thread main_thread;
			private System.Threading.Thread dispatch_thread;
			
			public DispatchThread (Cirrus.Thread mainThread)
			{
				this.main_thread = mainThread;
				this.dispatch_thread = new System.Threading.Thread (RunDispatch);
			}
			
			public void Run ()
			{
				ShouldRun = true;
				dispatch_thread.Start ();
			}
			
			public void Quit ()
			{
				ShouldRun = false;
				dispatch_thread.Interrupt ();
			}
			
			private void RunDispatch ()
			{
				while (ShouldRun) {
					try {
						main_thread.Enabled.WaitOne ();
						
						var dispatch = OnDispatch;
						if (dispatch != null)
							dispatch ();
						
					} catch (System.Threading.ThreadInterruptedException e) {
					}
				}
			}
		}
	}
}

