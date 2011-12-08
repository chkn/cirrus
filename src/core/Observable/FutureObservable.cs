/*
	FutureObservable.cs: IObservable implementations for Futures
  
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

	public static class IObservableExtensions {

		/// <summary>
		/// Returns a Future fulfilled by the next item returned by an IObservable.
		/// </summary>
		/// <remarks>
		///  It is important to remember that a Future essentially represents a one-off event,
		///   while an IObservable can represent a stream of many events. The Future returned by
		///   this method will represent the next item of that stream. You would need to call
		///   this method again after that Future is fulfilled to get subsequent items.
		/// </remarks>
		/// <returns>
		/// The future.
		/// </returns>
		/// <param name='observable'>
		/// Observable.
		/// </param>
		public static Future<T> Next<T> (this IObservable<T> observable)
		{
			return new ObserverFuture<T> (observable);
		}
	}

	public partial class Future : IObservable<Future> {
		
		public virtual IDisposable Subscribe (IObserver<Future> observer)
		{
			return new FutureRegistration (this, observer);
		}
		
		private sealed class FutureRegistration : IDisposable {

			private volatile bool disposed = false;

			public FutureRegistration (Future future, IObserver<Future> observer)
	        {
	            try {
					future.Wait ();
					if (!disposed) {
						observer.OnNext (future);
						observer.OnCompleted ();
					}
				}
				catch (Exception e) {
					if (!disposed)
						observer.OnError (e);
				}
	        }
			
			public void Dispose()
	        {
				disposed = true;
	        }
		}
	}
	
	public partial class Future<T> : IObservable<T> {
	
		public virtual IDisposable Subscribe (IObserver<T> observer)
		{
			return new FutureRegistration<T> (this, observer);
		}
		
		private sealed class FutureRegistration<T> : IDisposable {
		
			private volatile bool disposed = false;

			public FutureRegistration (Future<T> future, IObserver<T> observer)
			{
				try {
					var result = future.Wait ();
					if (!disposed) {
						observer.OnNext (result);
						observer.OnCompleted ();
					}
				}
				catch (Exception e) {
					if (!disposed)
						observer.OnError (e);
				}
			}
	
	        public void Dispose()
	        {
				disposed = true;
	        }
	    }
	}
	
	public partial class CompositeFuture {
	
		public override IDisposable Subscribe (IObserver<Future> observer)
		{
			var passthrough = new PassthroughObserver<Future> (observer, () => Status == FutureStatus.Fulfilled);
			
			foreach (var future in futures) {
				future.Subscribe (passthrough);
			}
			
			return passthrough;
		}
		
		/// <summary>
		/// Executes the specified iteration delegate as each constituent Future of this CompositeFuture is fulfilled.
		/// </summary>
		/// <remarks>
		/// This methodperforms a "deep" enumeration of constituent Futures, meaning that if any are also CompositeFutures, or
		///  other types of Futures that represent more than one state or value, each Future will enumerate its constituents
		///  as well. The iterator block may be an asynchronous method.
		/// </remarks>
		/// <returns>
		/// A Future indicating the completion of iteration, either because all constituent Futures were iterated, or
		///  because False was returned from an iteration invocation.
		/// </returns>
		/// <param name='iteration'>
		/// Iteration delegate. It will be called with each Future when it is fulfilled. It should return True to continue
		///  iterating or False to "break."
		/// </param>
		public virtual Future ForEach (Func<Future,Future<bool>> iteration)
		{
			var forEach = new ForEachObserver<Future> (iteration);
			using (Subscribe (forEach))
				forEach.IterationComplete.Wait ();
			return Fulfilled;
		}
	}
	
	public partial class FutureCollection<T> : IObservable<T> {
	
		public IDisposable Subscribe (IObserver<T> observer)
		{
			var initialCount = Count;
			int i = 0;
			
			var passthrough = new PassthroughObserver<T> (observer, () => {
				
				if (Count != initialCount)
					throw new InvalidOperationException ("The collection was modified after the observer was created.");
				
				return ++i >= initialCount;
			});
			
			foreach (var future in futures) {
				future.Subscribe (passthrough);
			}
			
			return passthrough;
		}
		
		/// <summary>
		/// Executes the specified iteration delegate for each fulfilled value of this FutureCollection.
		/// </summary>
		/// <remarks>
		/// This method performs a "deep" enumeration of constituent Futures, meaning that if any are also CompositeFutures, or
		///  other types of Futures that represent more than one state or value, each Future will enumerate its constituents
		///  as well. Considering how readily such composite futures are created, this is almost always the desired behavior.
		/// The iterator block may be an asynchronous method.
		/// </remarks>
		/// <returns>
		/// A Future indicating the completion of iteration, either because all constituent Futures were iterated, or
		///  because False was returned from an iteration invocation.
		/// </returns>
		/// <param name='iteration'>
		/// Iteration delegate. It will be called with each value when it is available. It should return True to continue
		///  iterating or False to "break."
		/// </param>
		public virtual Future ForEach (Func<T,Future<bool>> iteration)
		{
			var forEach = new ForEachObserver<T> (iteration);
			using (Subscribe (forEach))
				forEach.IterationComplete.Wait ();
			return Future.Fulfilled;
		}
	}
	
	public sealed class ForEachObserver<T> : IObserver<T> {
		
		public Future IterationComplete { get; private set; }
		
		private Func<T,Future<bool>> iteration; 
		
		public ForEachObserver (Func<T,Future<bool>> iteration)
		{
			this.IterationComplete = new Future ();
			this.iteration = iteration;
		}
		
		public void OnNext (T value)
		{
			if (IterationComplete.Status == FutureStatus.Pending) {
				try {
					var shouldContinue = iteration (value).Wait ();
					if (!shouldContinue)
						IterationComplete.Status = FutureStatus.Fulfilled;
					
				} catch (Exception e) {
					IterationComplete.Exception = e;
				}
			}
		}
		
		public void OnCompleted ()
		{
			IterationComplete.Status = FutureStatus.Fulfilled;
		}
		
		public void OnError (Exception exception)
		{
			IterationComplete.Exception = exception;
		}
		
	}
	
	public sealed class PassthroughObserver<T> : IObserver<T>, IDisposable {
		
		private bool disposed = false;
		
		private IObserver<T> target;
		private Func<bool> isComplete;
		
		public PassthroughObserver (IObserver<T> target, Func<bool> isComplete)
		{
			this.target = target;
			this.isComplete = isComplete;
		}
		
		public void OnNext (T value)
		{
			if (!disposed)
				target.OnNext (value);
		}
		
		public void OnCompleted ()
		{
			if (!disposed && isComplete ()) {
				target.OnCompleted ();
				disposed = true;
			}
		}
		
		public void OnError (Exception exception)
		{
			if (!disposed)
				target.OnError (exception);
		}
		
		public void Dispose ()
		{
			disposed = true;
		}
	}
}