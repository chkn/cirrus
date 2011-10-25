using System;
using NUnit.Framework;

using Cirrus;
using Cirrus.Test;

namespace Cirrus.Test.Cilc {
	
	public class DummyException : Exception {}
	
	[TestFixture]
	public class TargetILTests : TestsRequireScheduler
	{
		public static bool True = true;
		
		[Test]
		public void TestArgumentsAreMovedAfterContinuation ()
		{
			// params args
			var str0 = string.Concat ("I", "am", "from", "the", TestArgumentsAreMovedAfterContinuationAsync ().Wait ());
			
			// normal args
			var str1 = string.Concat ("Iam", "fromthe", TestArgumentsAreMovedAfterContinuationAsync ().Wait ());
			
			Assert.AreEqual (str0, "Iamfromthefuture", "#0");
			Assert.AreEqual (str1, "Iamfromthefuture", "#1");
			TestComplete ();
		}
		public Future<string> TestArgumentsAreMovedAfterContinuationAsync ()
		{
			Thread.Yield ();
			return "future";
		}

		
		[Test]
		public void TestCanCatchPreflightExceptions ()
		{
			try {
				TestCanCatchPreflightExceptionsAsync ().Wait ();
				
			} catch (DummyException e) {
				TestComplete ();
				return;
			}
			Assert.Fail ();
			TestComplete ();
		}
		private Future TestCanCatchPreflightExceptionsAsync ()
		{
			if (True)
				throw new DummyException ();
			Thread.Yield ();
			return Future.Fulfilled;
		}
		
		[Test]
		public void TestCanCatchPostflightExceptions ()
		{
			Future f = null;
			try {
				f = TestCanCatchPostflightExceptionsAsync ();
				f.Wait (); // < it should appear to code that exception is thrown here
				
			} catch (DummyException e) {
				Assert.IsNotNull (f, "#1");
				Assert.AreEqual (FutureStatus.Handled, f.Status, "#2");
				Assert.AreSame (e, f.Exception);
				TestComplete ();
				return;
			}
			Assert.Fail ();
			TestComplete ();
		}
		private Future TestCanCatchPostflightExceptionsAsync ()
		{
			Thread.Yield ();
			throw new DummyException ();
		}
		
		[Test]
		public void TestCanCallGenericAsyncMethod ()
		{
			object test = new object ();
			Assert.AreSame (TestCanCallGenericAsyncMethodAsync (test).Wait (), test, "#1");
			TestComplete ();
		}
		private Future<T> TestCanCallGenericAsyncMethodAsync<T> (T roundTripMe)
		{
			T local = roundTripMe;
			Thread.Yield ();
			return local;
		}
		
	}
}

