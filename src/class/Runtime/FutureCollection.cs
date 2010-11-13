/*
	FutureCollection.cs: A collection of future values
  
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
	
	public class FutureCollection<T> : ICollection<Future<T>> {
		private List<Future<T>> futures;
		
		public FutureCollection ()
		{
			futures = new List<Future<T>> ();	
		}
		
		public FutureCollection (IEnumerable<Future<T>> items)
		{
			futures = new List<Future<T>> (items);	
		}
		
		public AsyncEnumerator<T> GetEnumerator ()
		{
			return new AsyncEnumerator<T> (futures);
		}
		
		IEnumerator<Future<T>> IEnumerable<Future<T>>.GetEnumerator ()
		{
			return futures.GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotSupportedException ();
		}
		
		public void Add (Future<T> item)
		{
			futures.Add (item);
		}
		
		public void Add (T item)
		{
			futures.Add (item);	
		}

		public void Clear ()
		{
			futures.Clear ();
		}

		public bool Contains (Future<T> item)
		{
			return futures.Contains (item);
		}

		public void CopyTo (Future<T>[] array, int arrayIndex)
		{
			futures.CopyTo (array, arrayIndex);
		}

		public bool Remove (Future<T> item)
		{
			return futures.Remove (item);
		}

		public int Count {
			get { return futures.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}
		
		public Future<T> [] ToArray ()
		{
			return futures.ToArray ();	
		}
		
		public IList<Future<T>> ToList ()
		{
			return new List<Future<T>> (futures);
		}
		
		public Future<T []> ToFutureArray ()
		{
			(new CompositeFuture (futures.Cast<Future> (), FutureCompositors.WaitAll)).Wait ();
			return futures.Select (f => f.Value).ToArray ();	
		}
		
		public Future<List<T>> ToFutureList ()
		{
			(new CompositeFuture (futures.Cast<Future> (), FutureCompositors.WaitAll)).Wait ();
			return futures.Select (f => f.Value).ToList ();
		}
		
	}
}

