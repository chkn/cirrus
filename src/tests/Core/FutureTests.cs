using System;
using NUnit.Framework;

using Cirrus;
using Cirrus.Test;

namespace Cirrus.Test.Core {
	
	[TestFixture]
	public class FutureTests : TestsRequireScheduler {
		
		[Test]
		public void TestFutureCancelledExceptionInAsyncMethod ()
		{
			var f = TestFutureCancelledExceptionInAsyncMethodAsync ();
			f.Cancel ();
		}
		public Future TestFutureCancelledExceptionInAsyncMethodAsync ()
		{
			try {
				try {
					Thread.Yield ();
				}
				catch (FutureCancelledException e) {
					return Future.Fulfilled;
				}
				Assert.Fail ();
				return Future.Fulfilled;
			} finally {
				TestComplete ();
			}
		}
	}
}

