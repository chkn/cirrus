/*
	Thread.cs: Cirrus Fibers and Concurrency Runtime
  
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
using Interlocked = System.Threading.Interlocked;

#pragma warning disable 0420
// "A volatile field references will not be treated as volatile."
// This is because it is passed by ref to Interlocked methods, but they treat as volatile.

namespace Cirrus {

	public sealed class Thread {
		
		[ThreadStatic]
		internal static Thread current;
		public static Thread Current {
			get { 
				if (current == null)
					current = new Thread ();
				return current;
			}
		}

		// See RunSingleIteration for reasons why current_fiber MUST be declared volatile.
		internal volatile Future current_fiber;
		
		// This will be Set when fibers are scheduled.
		public System.Threading.ManualResetEvent Enabled { get; private set; }
		
		
		internal Thread ()
		{
			Enabled = new System.Threading.ManualResetEvent (false);
		}
		
		// Simple round-robin scheduler.
		public void RunLoop ()
		{
			while (true) {
				Enabled.WaitOne ();
				RunSingleIteration ();
			}
		}
		
		public void RunSingleIteration ()
		{
			// current_fiber MUST be declared volatile
			// (see http://msdn.microsoft.com/en-us/magazine/cc163715.aspx -
			//  "ECMA model lets the compiler eliminate the local variable and re-fetch the location on each use")
			var fiber = current_fiber;
			
			if (fiber == null)
				return;
			if (!fiber.IsScheduled)
				goto next_fiber;
			
			switch (fiber.Status) {
			
			case FutureStatus.Fulfilled:
			case FutureStatus.Handled:
				fiber.Unschedule ();
				goto next_fiber;

			case FutureStatus.PendingThrow:
				fiber.Status = FutureStatus.Throw;
				break;

			case FutureStatus.Throw:
				fiber.Exception.Rethrow ();
				break;
			}

			fiber.Resume ();

		next_fiber:
			// Only advance to fiber.Next if current_fiber == fiber
			// This check is necessary because during fiber.Resume(),
			//  fiber could have been unscheduled.
			Interlocked.CompareExchange<Future> (ref current_fiber, fiber.Next, fiber);
			
			// Reset is double-guarded to prevent race conditions with the test and the scheduling of another fiber.
			// current_fiber is declared volatile; I believe this is sufficient to make this correct.
			// ( see http://msdn.microsoft.com/en-us/magazine/cc163715.aspx -
			//    "Volatile memory accesses cannot be created, deleted, or moved")
			if (current_fiber == null) {
				Enabled.Reset ();
				if (current_fiber != null)
					Enabled.Set ();
			}
		}
		
		public static void Yield ()
		{
			throw new NotSupportedException ("This operation requires the postcompiler");
		}
	}
}

