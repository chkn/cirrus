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


namespace Cirrus {
	
	/// <summary>
	/// A Future that is implemented with a coroutine.
	/// </summary>
	/// <remarks>
	/// This class is used internally by the postcompiler.
	/// </remarks>
	public abstract class CoroutineFuture : Future {
		
		protected Future chained;
		protected int pc;
		
		public CoroutineFuture ()
		{
			Thread.Current.ScheduleFiber (this);
		}
		
		public override void Resume ()
		{
			if (chained != null && chained.Status == FutureStatus.Pending)
				return;

			Coroutine ();
		}
		
		protected abstract void Coroutine ();
	}
	
	/// <summary>
	/// A Future&lt;T&gt; that is implemented with a coroutine.
	/// </summary>
	/// <remarks>
	/// This class is used internally by the postcompiler.
	/// </remarks>
	public abstract class CoroutineFuture<T> : Future<T> {
		
		protected Future chained;
		protected int pc;
		
		public CoroutineFuture ()
		{
			Thread.Current.ScheduleFiber (this);
		}
		
		public override void Resume ()
		{
			if (chained != null && chained.Status == FutureStatus.Pending)
				return;

			Coroutine ();
		}
		
		protected abstract void Coroutine ();
	}
}