/*
	CompositeFuture.cs: Combine multiple Futures into one
  
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Cirrus {
	
	/// <summary>
	/// Given a set of constituent Futures, determines whether the CompositeFuture is fulfilled.
	/// </summary>
	/// <returns>
	/// True if the CompositeFuture is fulfilled, else False.
	/// </returns>
	public delegate bool FutureCompositor (IEnumerable<Future> futures);
	
	public static class FutureCompositors {
		public static readonly FutureCompositor WaitAll = futures => futures.All (f => f.Status == FutureStatus.Fulfilled);
		public static readonly FutureCompositor WaitAny = futures => futures.Any (f => f.Status == FutureStatus.Fulfilled);
	}
	
	/// <summary>
	/// A Future that is fulfilled based on a FutureCompositor being applied to a set of constituent Futures.
	/// </summary>
	/// <remarks>
	/// This class provides an easier and more efficient way to wait on multiple Futures at once.
	/// </remarks>
	public class CompositeFuture : Future {

		public FutureCompositor Compositor { get; private set; }
		public IEnumerable<Future> Futures { get; private set; }
		
		/// <summary>
		/// Creates a new instance of CompositeFuture that will be fulfilled when any of
		/// the constituent Futures are fulfilled.
		/// </summary>
		/// <param name="futures">
		/// The constituent Futures.
		/// </param>
		public CompositeFuture (IEnumerable<Future> futures) : this (futures, FutureCompositors.WaitAny)
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
			this.Futures = futures;
			this.Compositor = compositor;
			
			Thread.Current.ScheduleFiber (this);
		}
		
		public override void Resume ()
		{
			CompositeFutureException cfe = null;
			
			// If any future has an exception, then we handle that and throw it ourselves
			foreach (var hotFuture in Futures.Where (f => f.Exception != null)) {
				if (cfe == null) {
					cfe = new CompositeFutureException (hotFuture.Exception);
					Exception = cfe;
				}
				
				cfe.innerExceptions.Add (hotFuture.Exception);
				hotFuture.Status = FutureStatus.Handled;
			}
				
				
			if (!Futures.Any () || Compositor (Futures))
				Status = FutureStatus.Fulfilled;
		}
		
		/// <summary>
		///  Resets this CompositeFuture's Status to Pending and prunes its list of constituent Futures
		///  to only contain Pending Futures. This allows Wait to be called multiple times on this instance.
		/// </summary>
		public virtual void Reset ()
		{
			Futures = Futures.Where (f => f.Status == FutureStatus.Pending);
			Exception = null;
			Status = FutureStatus.Pending;
		}
	}
		
	public class CompositeFutureException : Exception {
		
		public IEnumerable<Exception> InnerExceptions {
			get { return innerExceptions; }
		}
		
		internal List<Exception> innerExceptions = new List<Exception> ();
		
		internal CompositeFutureException (Exception first) : base ("An exception occurred in one or more constituent Future(s)", first)
		{
		}
	}
}
