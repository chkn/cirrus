/*
	Future.cs: Asynchronous task primatives
  
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
	
	public enum FutureStatus {
		Pending = 0,
		Fulfilled = 1, //< If this value changes, must update TargetIL.cs
		Aborted = -1
	}
	
	/// <summary>
	/// Represents the future completion of an asynchronous operation.
	/// </summary>
	/// <remarks>
	/// The Future is simply an indicator that can be set to indicate
	/// that some expected state has come to pass. This could imply the completion of an
	/// asynchronous operation or the raising of an event. A Future may also represent
	/// a cooperative fiber or microthread if it is subclassed to contain code that will
	/// run cooperatively at intervals in conjunction with other fibers.
	/// 
	/// When the operation or event a Future represents occurs, or the fiber it represents
	/// terminates, the Future is said to be "fulfilled."  
	/// </remarks>
	public partial class Future {
		// (the rest of this class is in FutureAdapters.cs)
		
		/// <summary>
		/// The current status of this Future.
		/// </summary>
		public FutureStatus Status { get; set; }
		
		/// <summary>
		/// The time at which this Future should continue execution.
		/// </summary>
		/// <remarks>
		/// If this is non-null, then the runtime should not schedule this Future's
		/// Resume method for execution until on or after the specified time.
		/// Accordingly, this property is only relevant for Future subclasses that
		/// override the Resume method and pass True to the protected constructor.
		/// </remarks>
		public DateTime? WakeupTime { get; set; }
		
		/// <summary>
		/// The exception that occurred during the execution of this Future.
		/// </summary>
		/// <remarks>
		/// If this Future's Status is FutureStatus.Aborted and this Future was aborted due to
		/// an exception that was not caught during its execution, then this property
		/// will reference that Exception object. Note that it may be possible for the
		/// Future to be aborted for other reasons, in which case this property will be null.
		/// </remarks>
		public Exception Exception {
			get { return exception; }
			set {
				exception = value;
				if (value != null) 
					Status = FutureStatus.Aborted;
			}
		}
		private Exception exception;
		
		
		/// <summary>
		/// Creates a new, unfulfilled Future that will not schedule its Resume method for execution.
		/// </summary>
		public Future () : this (false)
		{	
		}
		
		/// <summary>
		/// Creates a new, unfulfilled Future that will optionally schedule its Resume method for execution.
		/// </summary>
		/// <remarks>
		/// Subclasses that wish to run code on each tick should override the Resume method and pass True for resumeImpl.
		/// </remarks>
		/// <param name="resumeImpl">
		/// A value of True indicates that this Future subclass implements its Resume method and it should be scheduled for execution.
		/// </param>
		protected Future (bool resumeImpl)
		{
			if (resumeImpl)
				Thread.fibers.Enqueue (this);	
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
		/// Resumes execution of this Future after it had yielded to other fibers.
		/// </summary>
		/// <remarks>
		/// This method is intended to be called by the runtime and should not normally
		/// be called from user code.
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown if this Future is not in a resumable state (e.g. fulfilled or aborted).
		/// </exception>
		public virtual void Resume ()
		{
			switch (Status) {
			
			case FutureStatus.Aborted:
				throw new InvalidOperationException ("This Future has been aborted");
				
			case FutureStatus.Fulfilled:
				throw new InvalidOperationException ("This Future is fulfilled");
			}
		}
	}
	
	/// <summary>
	/// Represents a future value or result of an asynchronous operation.
	/// </summary>
	/// <remarks>
	/// The Future is simply an indicator that can be set to indicate
	/// that some expected state has come to pass. This could imply the completion of an
	/// asynchronous operation or the raising of an event. A Future may also represent
	/// a cooperative fiber or microthread if it is subclassed to contain code that will
	/// run cooperatively at intervals in conjunction with other fibers.
	/// 
	/// In addition to the functionality provided by the non-generic Future class, the generic
	/// Future&lt;T&gt; class represents a value that may be the result of such an
    /// asynchronous operation or event. Basically, Future&lt;T&gt; represents a T that may not
    /// be known yet. A Future&lt;T&gt; is fulfilled if and when that T is available. 
	/// </remarks>
	public partial class Future<T> : Future {
		// (the rest of this class is in FutureAdapters.cs)
		
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
		/// Creates a new, unfulfilled Future&lt;T&gt; that will not schedule its Resume method for execution.
		/// </summary>
		public Future () : this (false)
		{	
		}
		
		/// <summary>
		/// Creates a new, unfulfilled Future&lt;T&gt; that will optionally schedule its Resume method for execution.
		/// </summary>
		/// <remarks>
		/// Subclasses that wish to run code on each tick should override the Resume method and pass True for resumeImpl.
		/// </remarks>
		/// <param name="resumeImpl">
		/// A value of True indicates that this Future&lt;T&gt; subclass implements its Resume method and it should be
		/// scheduled for execution.
		/// </param>
		protected Future (bool resumeImpl) : base (resumeImpl)
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

