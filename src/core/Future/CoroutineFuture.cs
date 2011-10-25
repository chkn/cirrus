/*
	CoroutineFuture.cs: Futures implemented with coroutines
  
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
using System.Diagnostics;

namespace Cirrus {
	
	/// <summary>
	/// A Future that is implemented with a coroutine.
	/// </summary>
	/// <remarks>
	/// This class is used internally by the postcompiler.
	/// </remarks>
	public abstract class CoroutineFuture : Future {
		
		protected Future chained;
		protected Thread thread;
		protected uint pc;
		protected ulong epc;
		
		public CoroutineFuture ()
		{
			SupportsCancellation = true;
			thread = Thread.Current;
		}
		
		protected bool Chain (Future f)
		{
			chained = f;
			
			// UBER IMPORTANTE! Must acquire reader lock on chained future's status to prevent race condition
			chained.status_lock.EnterReadLock ();
			try {
			
				if (chained.Status == FutureStatus.Pending) {
				
					Unschedule ();
					chained.OnComplete += Reschedule;
					return true;
				}
				
			}
			finally {
				chained.status_lock.ExitReadLock ();
			}
			return false;
		}
		
		protected void Reschedule (Future completed)
		{
			completed.OnComplete -= Reschedule;
			Schedule (thread);
		}
		
		public override void Cancel ()
		{
			if (chained != null)
				chained.OnComplete -= Reschedule;
			base.Cancel ();
			Schedule (thread);
		}
		
		protected void CheckException ()
		{
			if (this.Exception != null)
				throw this.Exception;
		}
		
		public abstract override void Resume ();
	}
	
	/// <summary>
	/// A Future&lt;T&gt; that is implemented with a coroutine.
	/// </summary>
	/// <remarks>
	/// This class is used internally by the postcompiler.
	/// </remarks>
	public abstract class CoroutineFuture<T> : Future<T> {
		
		protected Future chained;
		protected Thread thread;
		protected uint pc;
		protected ulong epc;
		
		public CoroutineFuture ()
		{
			SupportsCancellation = true;
			thread = Thread.Current;
		}
		
		protected bool Chain (Future f)
		{
			chained = f;
			
			// UBER IMPORTANTE! Must acquire reader lock on chained future's status to prevent race condition
			chained.status_lock.EnterReadLock ();
			try {
			
				if (chained.Status == FutureStatus.Pending) {
				
					Unschedule ();
					chained.OnComplete += Reschedule;
					return true;
				}
				
			}
			finally {
				chained.status_lock.ExitReadLock ();
			}
			return false;
		}
		
		protected void Reschedule (Future completed)
		{
			completed.OnComplete -= Reschedule;
			Schedule (thread);
		}
		
		public override void Cancel ()
		{
			if (chained != null)
				chained.OnComplete -= Reschedule;
			base.Cancel ();
			Schedule (thread);
		}
		
		protected void CheckException ()
		{
			if (this.Exception != null)
				throw this.Exception;
		}
		
		public abstract override void Resume ();
	}
}