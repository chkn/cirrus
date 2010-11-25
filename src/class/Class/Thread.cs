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
	
	public sealed class Thread {
		
		// Fibers will be resumed immediately if their remaining sleep time
		//  is less than this threshold
		private static TimeSpan sleep_threshold = TimeSpan.FromMilliseconds (5);
		
		public static Thread Current {
			get { 
				if (current == null)
					current = new Thread ();
				return current;
			}
		}
		
		[ThreadStatic]
		private static Thread current;
		
		
		private Queue<Future> activeFibers; 
		private LinkedList<Future> sleepingFibers;
		

		private Thread ()
		{
			activeFibers = new Queue<Future> ();
			sleepingFibers = new LinkedList<Future> ();
		}
		
		public void ScheduleFiber (Future fiber)
		{
			activeFibers.Enqueue (fiber);
		}
		
		// Simple round-robin scheduler.
		public void RunLoop ()
		{
			TimeSpan sleepTime;
			
			while (RunSingleIteration (out sleepTime)) {
				if (sleepTime.Ticks > 0)
					System.Threading.Thread.Sleep (sleepTime);
			}
		}
		
		// Returns false if there's no more stuff to do
		public bool RunSingleIteration (out TimeSpan sleepTime)
		{
			Future fiber;
			bool active = false;
			var now = DateTime.Now;
			
			// Figure out which fiber to resume...
			if (!ShouldResume (now, out fiber, out sleepTime)) {

				while (activeFibers.Count != 0) {
					TimeSpan newSleepTime;
					fiber = activeFibers.Dequeue ();
					
					if (ShouldSleep (now, fiber, out newSleepTime)) {
						sleepTime = new TimeSpan (Math.Min (sleepTime.Ticks, newSleepTime.Ticks));
					} else {
						active = true;
						break;
					}
				}
				
				if (!active)
					return (sleepingFibers.Count != 0);
			}
			
			sleepTime = default (TimeSpan);
			
			//*--------------------------------------*
			
			switch (fiber.Status) {
			
			case FutureStatus.Fulfilled:
			case FutureStatus.Handled:
				return true;
			
			case FutureStatus.PendingThrow:
				fiber.Status = FutureStatus.Throw;
				activeFibers.Enqueue (fiber);
				return true;
				
			case FutureStatus.Throw:
				throw fiber.Exception;
				
			}
			
			// Resume fiber..
			fiber.WakeupTime = null;
			fiber.Resume ();
			
			activeFibers.Enqueue (fiber);	
			return true;
		}
		
		public static void Sleep (uint millis)
		{
			throw new NotSupportedException ("This operation requires the postcompiler");	
		}
		
		private bool ShouldSleep (DateTime when, Future toSleep, out TimeSpan sleepTime)
		{
			sleepTime = default (TimeSpan);
			
			// We don't even want to sleep
			if (toSleep.WakeupTime == null)
				return false;
			
			var wakeupTime = toSleep.WakeupTime.Value;
			sleepTime = wakeupTime.Subtract (when);
			
			// We're not sleeping long enough, don't bother.
			if (sleepTime.Subtract (sleep_threshold).Ticks <= 0)
				return false;
			
			// We gotta sleep... order the fiber in the sleeping list
			
			if (sleepingFibers.Count == 0) {
				sleepingFibers.AddFirst (toSleep);
				return true;
			}
			
			if (wakeupTime <= sleepingFibers.First.Value.WakeupTime.Value) {
				sleepingFibers.AddFirst (toSleep);
				return true;
			}
			
			var currentSleeper = sleepingFibers.Last;
			
			while (currentSleeper.Value.WakeupTime.Value > wakeupTime)
				currentSleeper = currentSleeper.Previous;
			
			sleepingFibers.AddAfter (currentSleeper, toSleep);
			return true;
		}
		
		/// <summary>
		/// Checks the list of sleeping fibers.
		/// If one is ready to wake up, returns true and toResume will be set.
		/// If none are ready, returns false and toSleep will be set.
		/// </summary>
		/// <param name="when">
		/// Will check for fibers ready to resume at the given <see cref="DateTime"/>.
		/// </param>
		/// <param name="toResume">
		/// The soonest <see cref="Future"/> to awaken.
		/// </param>
		/// <param name="toSleep">
		/// A <see cref="TimeSpan"/> indicating how much time the soonest Fiber to awaken still has to sleep.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> indicating whether to resume the Fiber output as the first argument.
		/// </returns> 
		private bool ShouldResume (DateTime when, out Future toResume, out TimeSpan toSleep)
		{
			toResume = default (Future);
			toSleep = default (TimeSpan);
			
			if (sleepingFibers.First == null)
				return false;
			
			toResume = sleepingFibers.First.Value;
			
			var wakeupTime = toResume.WakeupTime;
			toSleep = wakeupTime.Value.Subtract (when);
			
			if (toSleep.Subtract (sleep_threshold).Ticks <= 0) {
				sleepingFibers.RemoveFirst ();
				return true;
			}	
				
			return false;
		}
	}
}

