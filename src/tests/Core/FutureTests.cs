using System;
using NUnit.Framework;

using Cirrus;
using Cirrus.Test;

namespace Cirrus.Test.Core {
	
	[TestFixture]
	public class FutureTests : TestsRequireScheduler {
		
		[Test]
		public void TestFutureCancelledException ()
		{
			var f = TestFutureCancelledExceptionAsync ();
			f.Cancel ();
		}
		public Future TestFutureCancelledExceptionAsync ()
		{
			try {
				try {
					Thread.Yield ();
					Assert.Fail ("#1");
				}
				catch (FutureCancelledException) {
					return Future.Fulfilled;
				}
				Assert.Fail ("#2");
				return Future.Fulfilled;
			} finally {
				TestComplete ();
			}
		}

		[Test]
		public void TestFutureCancelledExceptionInChained ()
		{
			var f = TestFutureCancelledExceptionInChainedAsync ();
			f.Cancel ();
		}
		public Future TestFutureCancelledExceptionInChainedAsync ()
		{
			var i = 0;
			try {
				try {
					Future.MillisecondsFromNow (1000).Wait ();
					Assert.Fail ("#2");
				}
				catch (FutureCancelledException) {
					return Future.Fulfilled;
				}
				Assert.Fail ("#3");
				return Future.Fulfilled;
			} finally {
				Assert.AreEqual (0, i, "#4");
				i++;
				TestComplete ();
			}
		}
	}
}

