/*
	TimeoutFuture.cs: A Future representing a set period of time
  
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
using Timer = System.Threading.Timer;

namespace Cirrus
{
	public sealed class TimeoutFuture : Future
	{
		volatile Timer timer;

		public TimeoutFuture (uint millis)
		{
			timer = new Timer (this.Callback, null, millis, 0);
			SupportsCancellation = true;
		}

		public override void Cancel ()
		{
			if (timer == null)
				return;

			status_lock.EnterWriteLock ();
			try {
				if (timer == null)
					return;

				timer.Dispose ();
				timer = null;
				base.Cancel ();
			} finally {
				status_lock.ExitWriteLock ();
			}
		}

		// This will occur on a different thread.
		void Callback (object @null)
		{
			status_lock.EnterWriteLock ();
			try {
				if (timer == null)
					return;

				timer.Dispose ();
				timer = null;
				Status = FutureStatus.Fulfilled;

			} finally {
				status_lock.ExitWriteLock ();
			}
		}
	}
}

