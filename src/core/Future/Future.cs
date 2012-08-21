/*
	Future.cs: Asynchronous task primatives
  
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
using System.Collections.Generic;

using Interlocked = System.Threading.Interlocked;
using Monitor = System.Threading.Monitor;
using ReaderWriterLockSlim = System.Threading.ReaderWriterLockSlim;

#pragma warning disable 0420
// "A volatile field reference will not be treated as volatile."
// OK- because Interlocked methods treat the field as volatile.

namespace Cirrus {
	
	public enum FutureStatus : short {

		// This Future is not yet Fulfilled, but may yet become so
		Pending = 0,
		
		// If any of the following values change, must update TargetIL.cs!

		// This Future is Fulfilled. It has reached its desired end state.
		Fulfilled =  1,

		// An Exception has occured, but the Future is pending a chance to handle it on the run loop
		PendingThrow = -3,

		// An Exception has occured and will not be handled by this Future
		Throw = -2,

		// An Exception has occured, but has been handled by a chained Future
		Handled = -1,
	};

	public static class FutureStatusEx {
		
		public static bool IsAborted (this FutureStatus status)
		{
			return ((short)status) < 0;	
		}
	}
	
	/// <summary>
	/// Represents the future completion of an asynchronous operation.
	/// </summary>
	/// <remarks>
	/// The Future is simply an indicator that can be set to indicate
	/// that some expected state has come to pass. This could imply the completion of an
	/// asynchronous operation or the raising of an event.
	/// <para/>
	/// A Future may also represent a cooperative fiber or microthread if it is
	/// subclassed to contain code that will run cooperatively at intervals in conjunction
	/// with other fibers. To do this, override the <see cref="Resume()"/> method and call
	/// <see cref="Schedule"/> to schedule that method to be called.
	/// <para/>
	/// When the operation or event a Future represents occurs, or the fiber it represents
	/// terminates, the Future is said to be "fulfilled."  
	/// </remarks>
	public partial class Future {
		// (the rest of this class is in FutureAdapters.cs and FutureObservable.cs)
		
		/// <summary>
		/// The current status of this Future.
		/// </summary>
		/// <remarks>
		/// This property may be set from any thread. Note that setting this property to Cancelled directly
		///  will not cancel the operation this Future represents. Instead, call <see cref="Cancel()"/>
		/// </remarks>
		public FutureStatus Status {
			get {
				// I don't think we need to lock status_lock here. Simply reading the field is guaranteed atomic by the CLR.
				return status;
			}
			set {
				status_lock.EnterWriteLock ();
				try {
					if ((status == FutureStatus.Fulfilled && value != FutureStatus.Fulfilled) ||
						(status == FutureStatus.Handled && value != FutureStatus.Handled))
						throw new InvalidOperationException ("Cannot change Status once it is Fulfilled or Handled.");

					if ((value == FutureStatus.Fulfilled && status != FutureStatus.Fulfilled) || 
					    (value == FutureStatus.Throw && status != FutureStatus.Throw)) {
						
						status = value;
						if (OnComplete != null)
							OnComplete (this);
						
					} else {
					
						status = value;
					}
				}
				finally {
					status_lock.ExitWriteLock ();
				}
			}
		}
		private FutureStatus status;
		internal ReaderWriterLockSlim status_lock = new ReaderWriterLockSlim (System.Threading.LockRecursionPolicy.SupportsRecursion);

		/// <summary>
		/// Raised when this Future will not execute further.
		/// </summary>
		/// <remarks>
		/// This event will be fired when the Status property is set to Fulfilled or Throw, or by
		///  the Cancel method. This event is used internally for chaining (i.e. one future waiting
		///  on another) and sleeping. Callbacks can occur on any thread.
		/// </remarks>
		internal event Action<Future> OnComplete;
		
		/// <summary>
		/// The exception that occurred during the execution of this Future.
		/// </summary>
		/// <remarks>
		/// The base implementation of this property sets this instance's Status to FutureStatus.Throw.
		/// This default behavior immediately causes the OnComplete event to fire, which schedules any chained
		/// Futures that may wish to handle the exception. If a subclass wishes to have the first chance to handle
		/// the exception, it must override this property and first set Status = FutureStatus.PendingThrow, and schedule
		/// itself on the appropriate run loop before calling this base implementation.
		/// </remarks>
		/// <exception cref="InvalidOperationException">If changing this property after it has been set.</exception>
		public virtual Exception Exception {
			get { return exception; }
			set {
				if (exception == value)
					return;

				exception = value;
				if (Status != FutureStatus.PendingThrow)
					Status = FutureStatus.Throw;
			}
		}
		private Exception exception;
		
		// Each Future can potentially be a node in a circular, doubly-linked list of scheduled Futures
		//  owned by a particular Thread object.
		public Future Next {
			get { return next; }
		}
		public bool IsScheduled {
			get { return previous != null; }
		}
		private volatile Future next, previous;
		
		/// <summary>
		/// Creates a new, unfulfilled Future.
		/// </summary>
		public Future ()
		{	
		}
		
		/// <summary>
		/// Resets this Future to Pending.
		/// </summary>
		protected virtual void InternalReset ()
		{
			status = FutureStatus.Pending;
			exception = null;
		}
		
		protected void Schedule ()
		{
			Schedule (Thread.Current);	
		}
		
		// This is thread-safe.
		protected internal void Schedule (Thread thread)
		{
			Future current, target, actual, potential;
			
		top:
			actual = Interlocked.CompareExchange<Future> (ref this.previous, this, null);
			if (actual != null)
				return; // already scheduled

			// Scheduling algorithm:
			// This must be done carefully, since other Futures may be concurrently (un)scheduling themselves
			// from different threads.
		
			// 1. Assume there are no currently scheduled fibers.
			//  Maintain circularity of the scheduler
			this.next = this;
			
		set_current_to_this:
			// 2. Slip ourselves in to the thread.current_fiber if there isn't any.
			//  Either way, also grabs current = thread.current_fiber
			current = Interlocked.CompareExchange<Future> (ref thread.current_fiber, this, null);
			
			// 2a. If there actually wasn't any previously scheduled fiber,
			//  we have to wake the thread up...
			if (current == null) {
				thread.Enabled.Set ();
				return;
			}
			
		//acquire_new_target:
			// 3. We really want the fiber scheduled *previously* to the current one
			//     (less likely to be unscheduled, for one)
			target = current.previous;
			
		find_new_current:
			// 3a. If we're unlucky, the current fiber was unscheduled during this time.
			//   Negotiate for a new current fiber (possibly this one)
			while (target == null) {
				potential = current.next;
				actual = Interlocked.CompareExchange<Future> (ref thread.current_fiber, potential, current);
				current = (actual == current? potential : actual);
				if (current == null)
					goto set_current_to_this;
				
				target = current.previous;	
			}
			
		begin_update:
			// 4. We can still update our fields without worry here because nobody points to us yet.
			this.next = current;
			this.previous = target;
			
			// 4a. Set current.previous = this
			actual = Interlocked.CompareExchange<Future> (ref current.previous, this, target);
			if (actual == null) {
				// current was unscheduled
				target = null;
				goto find_new_current;
			}
			if (actual != target) {
				// some other fiber beat us to the chase; or target unscheduled itself
				target = actual;
				goto begin_update;
			}
			
			// 4b. Set target.next = this
			actual = Interlocked.CompareExchange<Future> (ref target.next, this, current);
			if (actual == null) {
				// was only fiber and unscheduled itself
				goto top;
			}
			if ((actual != current) && (actual != this) && (actual != this.previous)) {
				// some other fiber beat us to the chase,
				// OR, more likely, a fiber is in the process of unscheduling itself b/w current & target.
				// this is ok if our previous has been updated too; otherwise ???
				throw new Exception ("should not be reached");
			}
		}
		
		// This method is thread-safe, however if this method is called from a different thread than
		//  this Future is scheduled on, it is not guaranteed that this Future will not be scheduled for execution
		//  until this method returns.
		// Additionally, unscheduling a Future from one Thread and then scheduling it to a DIFFERENT thread
		//  is currently not supported due to a race condition.
		protected internal void Unschedule ()
		{
			Future actual;
			var prev = Interlocked.Exchange<Future> (ref previous, null);
			if (prev == null) {
				// already unscheduled
				return;
			}
			
		unlink_next:
			// set next = null if next == this
			actual = Interlocked.CompareExchange<Future> (ref next, null, this);
			if (actual == this || actual == null) {
				// we were the only ones scheduled.. Thread.current_fiber will get set to null naturally.
				return;
			}

			// Set next.previous = previous
			actual = Interlocked.CompareExchange<Future> (ref actual.previous, prev, this);
			if (actual != this) {
				// someone just scheduled themselves b/w us and next (I think.. is there a possibility this would loop forever?)
				this.next = actual;
				goto unlink_next;
			}
			
			
			// Set previous.next = next
			Interlocked.CompareExchange<Future> (ref prev.next, next, this);
			
		}
		
		/// <summary>
		/// Causes the calling fiber to yield execution to other fibers until this Future is fulfilled.
		/// </summary>
		/// <remarks>
		/// This method requires the postcompiler be run on the CIL module containing the caller.
		/// </remarks>
		/// <exception cref="NotSupportedException">
		/// If the CIL module containing the code that calls this method was not correctly postcompiled.
		/// </exception>
		public void Wait ()
		{
			throw new NotSupportedException ("This operation requires the postcompiler");
		}
		
		/// <summary>
		/// If this Future is scheduled as a fiber, this method is called to resume its execution after
		/// it had yielded to other fibers.
		/// </summary>
		/// <remarks>
		/// This method is intended to be called by the runtime and should not normally
		/// be called from user code.
		/// </remarks>
		public virtual void Resume ()
		{	
		}
		
		public virtual bool SupportsCancellation { get; protected set; }
		
		/// <summary>
		/// Attempts to cancel this Future, preventing any continuation or further processing
		///  toward its fulfillment.
		/// </summary>
		/// <description>
		/// If a Future's fulfillment entails a long and/or expensive operation, it might be beneficial
		///  to offer the option to cancel this operation. A consumer may use the SupportsCancellation
		///  property to determine if the Future offers this option.
		/// <para/>
		/// Coroutine Futures will call Cancel on any chained future and then cause a FutureCancelledException
		///  to be thrown from the continuation point. This allows finally blocks to run as expected.
		///  It may make sense for the coroutine to catch this exception depending on its semantic.
		///   Otherwise, the exception will propagate to chained futures or be thrown from the run loop as usual.
		/// <para/>
		/// It is legal to call this method regardless of the status of this Future.
		///  If it is not Pending, no action is taken.
		/// </description>
		/// <remarks>
		/// The base implementation contained in the Future class throws a NotSupportedException
		///  if SupportsCancellation returns False. Otherwise, it performs the tasks necessary to set
		///  this Future's status to Cancelled. If the specific subclass of Future must perform additional
		///  tasks to, for example, cancel an ongoing operation, it should override this method and execute those tasks
		///  BEFORE calling the base implementation. However, if the subclass is chained on another Future, it should simply
		///  call that Future's Cancel method and NOT call its own base implementation. Implementors should also ensure that,
		///  if the task has already completed, been faulted, or been canceled, subsequent calls to this method are no-ops.
		/// </remarks>
		/// <exception cref="NotSupportedException">
		/// Is thrown when this Future does not support cancellation, as reported by the SupportsCancellation property.
		/// </exception>
		public virtual void Cancel ()
		{
			if (!SupportsCancellation)
				throw new NotSupportedException ("This Future does not support cancellation");
			
			if (Status != FutureStatus.Pending)
				return;
			
			Exception = new FutureCancelledException (this);
		}
	}
	
	/// <summary>
	/// Represents a future value or result of an asynchronous operation.
	/// </summary>
	/// <remarks>
	/// The Future is simply an indicator that can be set to indicate
	///  that some expected state has come to pass. This could imply the completion of an
	///  asynchronous operation or the raising of an event. A Future may also represent
	///  a cooperative fiber or microthread if it is subclassed to contain code that will
	///  run cooperatively at intervals in conjunction with other fibers.
	/// <para/>
	/// In addition to the functionality provided by the non-generic Future class, the generic
	///  Future&lt;T&gt; class represents a value that may be the result of such an
    ///  asynchronous operation or event. Basically, Future&lt;T&gt; represents a T that may not
    ///  be known yet. A Future&lt;T&gt; is fulfilled if and when that T is available. 
	/// </remarks>
	public partial class Future<T> : Future {
		// (the rest of this class is in FutureAdapters.cs, FutureObservable.cs, and FutureEventBridge.cs)
		
		/// <summary>
		/// The value represented by this Future&lt;T&gt;.
		/// </summary>
		/// <remarks>
		/// The Status of this Future&lt;T&gt; must be FutureStatus.Fulfilled in order to read this property.
		/// Setting this property also sets the Status to FutureStatus.Fulfilled.
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// If the Status is not FutureStatus.Fulfilled.
		/// </exception>
		public T Value {
			get {
				if (Status != FutureStatus.Fulfilled)
					throw new InvalidOperationException ("Future not fulfilled");
				return return_value;
			}
			set {
				return_value = value;
				Status = FutureStatus.Fulfilled;
			}
		}
		private T return_value;
		
		/// <summary>
		/// Creates a new, unfulfilled Future&lt;T&gt;.
		/// </summary>
		public Future ()
		{	
		}
		
		/// <summary>
		/// Causes the calling fiber to yield execution to other fibers until this Future is fulfilled and returns
		/// its result.
		/// </summary>
		/// <remarks>
		/// This method requires the postcompiler be run on the CIL module containing the caller.
		/// </remarks>
		/// <returns>
		/// The result of the asynchronous operation.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// If the CIL module containing the code that calls this method was not correctly postcompiled.
		/// </exception>
		public new T Wait ()
		{
			throw new NotSupportedException ("This operation requires the postcompiler");
		}
	}
}

