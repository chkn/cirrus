/*
	AsyncEnumerator.cs: Asynchronous iteration with foreach
  
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
	/// An enumerator that provides asynchronous iteration over a collection of Future values.
	/// </summary> 
	/// <remarks>
	/// This enumerator may be used with a foreach loop. The foreach loop will yield control to other
	/// fibers until a Future in the bound enumerable is Fulfilled. Then, it will perform
	/// a single loop iteration given that Future's value.
	/// </remarks>
	public sealed class AsyncEnumerator<T> : Future<T> {
		
		private HashSet<Future<T>> items;
		private int position, total;
		
		/// <summary>
		/// Creates a new instance of the AsyncEnumerator&lt;T&gt; class.
		/// </summary>
		/// <param name="boundEnumerable">
		/// An <see cref="IEnumerable<Future<T>>"/> over which this AsyncEnumerator will iterate.
		/// </param>
		public AsyncEnumerator (IEnumerable<Future<T>> boundEnumerable)
		{
			this.items = new HashSet<Future<T>> (boundEnumerable);
			this.total = items.Count;
			this.position = 0;
			
			Thread.Current.ScheduleFiber (this);
		}
		
		// In this case, calling get_Current is equivalent to calling Wait ()
		public T Current {
			get {
				throw new NotSupportedException ("This operation requires the postcompiler");
			}
		}
		
		// FIXME: We may need to reschedule this fiber if there was a continuation between
		//  a get_Current and a MoveNext.
		public bool MoveNext ()
		{
			if (position == total)
				return false;
			
			Exception = null;
			Status = FutureStatus.Pending;
			
			position++;
			return true;
		}
		
		public override void Resume ()
		{
			var next = items.FirstOrDefault (f => f.Status == FutureStatus.Fulfilled);
			if (next != null) {
				items.Remove (next);
				Value = next.Value;
			}
		}

		public void Reset ()
		{
			throw new NotSupportedException ();
		}
	}
}

