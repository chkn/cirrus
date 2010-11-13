/*
	Thread.cs: Cirrus Fibers and Concurrency Runtime
  
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
using System.Collections.Generic;

namespace Cirrus {
	
	public static class Thread {
		
		[ThreadStatic]
		internal static Queue<Future> fibers;
		
		// Fibers will be resumed immediately if their remaining sleep time
		//  is less than this threshold
		private static TimeSpan sleep_threshold = TimeSpan.FromMilliseconds (5);
		
		public static void Init ()
		{
			fibers = new Queue<Future> ();	
		}
		
		// Simple round-robin scheduler.
		public static void RunLoop ()
		{
			TimeSpan sleepTime;
			
			while (RunSingleIteration (out sleepTime)) {
				if (sleepTime.Ticks != 0)
					System.Threading.Thread.Sleep (sleepTime);
			}
		}
		
		// Returns false if there's no more stuff to do
		public static bool RunSingleIteration (out TimeSpan sleepTime)
		{
			sleepTime = default (TimeSpan);
			if (fibers.Count == 0)
				return false;
			
			var current = fibers.Dequeue ();
			switch (current.Status) {
				
			case FutureStatus.Fulfilled:
				return true;
				
			// case FutureStatus.Aborted:
				//FIXME: Add exception handling magic here
			}
			
			if (!CheckSleep (current, out sleepTime)) {
				current.WakeupTime = null;
				current.Resume ();
			}
			
			fibers.Enqueue (current);
			return true;
		}
		
		// Returns true if the fiber should sleep, false if it should resume now
		//  If return value is true, physicalSleepTime is the amt. of time (if any)
		//  that the whole physical thread may sleep
		// FIXME: More optimization
		//  - What if a Future is chained on a sleeping Future?
		//  - Keep track of sleeping fibers in a separate data structure to avoid looping through each fiber
		private static bool CheckSleep (Future current, out TimeSpan physicalSleepTime)
		{
			physicalSleepTime = default (TimeSpan);
			var currentWakeup = current.WakeupTime;
			
			// current fiber doesn't want to sleep
			if (currentWakeup == null)
				return false;
			
			var now = DateTime.Now;
			var sleepTime = currentWakeup.Value.Subtract (now);
			
			// no more than sleep_threashold sleep time left
			if (sleepTime.Subtract (sleep_threshold).Ticks <= 0)
				return false;
			
			// current fiber wants to sleep... check to see if we can sleep the whole physical thread
			foreach (var fiber in fibers) {
				var wakeup = fiber.WakeupTime;
				
				// there is another fiber that is not sleeping.. do not sleep physical thread
				if (wakeup == null)
					return true;
				
				// there is another fiber that is sleeping for less time than this one. do not sleep physical thread (yet)
				if (wakeup.Value < currentWakeup.Value)
					return true;
			}
			
			// sleep whole physical thread
			physicalSleepTime = sleepTime;
			return true;
		}
		
		public static void Sleep (uint millis)
		{
			throw new NotSupportedException ("This operation requires the postcompiler");	
		}
	}
}

