/*
	CompositeFuture.cs: Combine multiple Futures into one
  
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System.Threading;

namespace Cirrus {
	
	/// <summary>
	/// Given a set of constituent Futures, determines whether the CompositeFuture is fulfilled.
	/// </summary>
	/// <remarks>
	/// This function should not modify state and should be reentrant, as it may be called from different threads.
	/// </remarks>
	/// <returns>
	/// True if the CompositeFuture is fulfilled, else False.
	/// </returns>
	public delegate bool FutureCompositor (IEnumerable<Future> futures);
	
	public static class FutureCompositors {
		// According to MSDN, all public static members of System.Linq.Enumerable are thread safe,
		//  and we should be safe from concurrent modification of the futures list during enumeration because of reset_lock.
		public static readonly FutureCompositor WaitAll = futures => futures.All (f => f.Status == FutureStatus.Fulfilled);
		public static readonly FutureCompositor WaitAny = futures => futures.Any (f => f.Status == FutureStatus.Fulfilled);
	}
	
	/// <summary>
	/// A Future that is fulfilled based on a FutureCompositor being applied to a set of constituent Futures.
	/// </summary>
	/// <remarks>
	/// This class provides an easy and efficient way to wait on multiple Futures at once.
	/// </remarks>
	public partial class CompositeFuture : Future {
		// (the rest of this class in FutureObservable.cs)

		public FutureCompositor Compositor { get; private set; }
		
		private List<Future> futures;
		
		// FIXME: Why can't we just use status_lock?
		private ReaderWriterLockSlim reset_lock = new ReaderWriterLockSlim (LockRecursionPolicy.SupportsRecursion);
		
		public CompositeFuture (IEnumerable<Future> futures)
			: this (futures, FutureCompositors.WaitAny)
		{
		}
		
		/// <summary>
		/// Creates a new instance of CompositeFuture given a set of constituent Futures and a <see cref="FutureCompositor"/>.
		/// </summary>
		/// <param name="futures">
		/// The constituent Futures.
		/// </param>
		/// <param name="compositor">
		/// The <see cref="FutureCompositor"/> that will determine when this CompositeFuture is fulfilled.
		/// </param>
		public CompositeFuture (IEnumerable<Future> futures, FutureCompositor compositor)
		{
			this.futures = new List<Future> (futures);
			this.Compositor = compositor;
			
			ChainAll (this.futures, CheckFuture);
			CheckFulfilled ();
		}
		
		/// <summary>
		///  Prunes this CompositeFuture's list of constituent Futures and potentially resets its Status to Pending.
		/// </summary>
		/// <remarks>
		///  This method will remove all Futures from this CompositeFuture that are not Pending.
		///   Then, this CompositeFuture's status is reset to Pending, depending on the result of the compositor.
		///   This allows Wait to be called multiple times on this instance.
		/// </remarks>
		public void Reset ()
		{
			InternalReset ();
		}
		
		/// <summary>
		/// Gets the fulfilled Futures from this CompositeFuture and calls Reset in an atomic fashion.
		/// </summary>
		/// <returns>
		/// The fulfilled futures.
		/// </returns>
		public IEnumerable<Future> GetFulfilledAndReset ()
		{
			// allows us to get a sane snapshot of fulfilled futures.
			reset_lock.EnterWriteLock ();
			try {
				var fulfilled = futures.Where (f => f.Status == FutureStatus.Fulfilled).ToArray ();
				InternalReset ();
				return fulfilled;
			}
			finally {
				reset_lock.ExitWriteLock ();
			}
		}
		
		protected override void InternalReset ()
		{
			reset_lock.EnterWriteLock ();
			try {
				for (var i = 0; i < futures.Count; i++) {
					
					if (futures [i].Status != FutureStatus.Pending) {
						futures [i].OnComplete -= CheckFuture;
						futures.RemoveAt (i);
					}
				}
				
				if (!CheckFulfilled ())
					base.InternalReset ();
			}
			finally {
				reset_lock.ExitWriteLock ();
			}
		}
		
		private void CheckFuture (Future chained)
		{
			reset_lock.EnterReadLock ();
			try {
				
				// if we're already fulfilled, then we're done!
				if (this.Status == FutureStatus.Fulfilled)
					return;
				
				// eat constituent future exceptions and throw an aggregate exception
				if (chained.Exception != null) {
					
					if (this.Exception == null) {
						this.Exception = new AggregateException (chained.Exception);
						
					} else {
						
						var agg = new AggregateException (this.Exception, chained.Exception);
						this.Exception = agg.Flatten ();
					}
					
					chained.Status = FutureStatus.Handled;
				}
	
				CheckFulfilled ();
				
			}
			finally {
				reset_lock.ExitReadLock ();
			}
		}
		
		private bool CheckFulfilled ()
		{
			if ((Exception == null) && (futures.Count == 0 || Compositor (futures))) {
				this.Status = FutureStatus.Fulfilled;
				return true;
			}
			return false;
		}
		
		internal static int ChainAll<F> (IEnumerable<F> futures, Action<Future> onComplete)
			where F : Future
		{
			var count = 0;
			foreach (var future in futures) {
				
				future.status_lock.EnterReadLock ();
				try {
					if (future.Status == FutureStatus.Pending)
						future.OnComplete += onComplete;
				}
				finally {
					future.status_lock.ExitReadLock ();
				}
				
				count++;
			}
			
			return count;
		}
	}
}
