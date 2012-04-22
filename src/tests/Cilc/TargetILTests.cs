using System;
using NUnit.Framework;

using Cirrus;
using Cirrus.Test;

namespace Cirrus.Test.Cilc {
	
	public class DummyException : Exception {
		public DummyException (string message) : base (message) {}
		public DummyException () { }
	}
	
	[TestFixture]
	public class TargetILTests : TestsRequireScheduler
	{
		public static bool True = true;
		
		[Test]
		public void TestArgumentsAreMovedAfterContinuation ()
		{
			try {
				// params args
				var str0 = string.Concat ("I", "am", "from", "the", TestArgumentsAreMovedAfterContinuationAsync ().Wait ());
				
				// normal args
				var str1 = string.Concat ("Iam", "fromthe", TestArgumentsAreMovedAfterContinuationAsync ().Wait ());
				
				Assert.AreEqual (str0, "Iamfromthefuture", "#0");
				Assert.AreEqual (str1, "Iamfromthefuture", "#1");
			} finally {
				TestComplete ();
			}
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
				try {
					TestCanCatchPreflightExceptionsAsync ().Wait ();
					
				} catch (DummyException e) {
					return;
				}
				Assert.Fail ();
			} finally {
				TestComplete ();
			}
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
			try {
				Future f = null;
				try {
					f = TestCanCatchPostflightExceptionsAsync ();
					f.Wait (); // < it should appear to code that exception is thrown here
					
				} catch (DummyException e) {
					Assert.IsNotNull (f, "#1");
					Assert.AreEqual (FutureStatus.Handled, f.Status, "#2");
					Assert.AreSame (e, f.Exception);
					return;
				}
				Assert.Fail ();
			} finally {
				TestComplete ();
			}
		}
		private Future TestCanCatchPostflightExceptionsAsync ()
		{
			Thread.Yield ();
			throw new DummyException ();
		}
		
		[Test]
		public void TestCanCallGenericAsyncMethod ()
		{
			try {
				object test = new object ();
				Assert.AreSame (TestCanCallGenericAsyncMethodAsync (test).Wait (), test, "#1");
			} finally {
				TestComplete ();
			}
		}
		private Future<T> TestCanCallGenericAsyncMethodAsync<T> (T roundTripMe)
		{
			T local = roundTripMe;
			Thread.Yield ();
			return local;
		}
		
		[Test]
		public void TestCanReturnFromUsingBlock ()
		{
			try {
				var foo = "bar";
				Assert.AreSame (foo, TestCanReturnFromUsingBlockAsync (foo).Wait ());
			} finally {
				TestComplete ();
			}
		}
		private Future<string> TestCanReturnFromUsingBlockAsync (string foo)
		{
			Thread.Yield ();
			using (var dummy = new System.IO.MemoryStream ())
				return foo;
		}
		
		[Test]
		public void TestCanRethrowException ()
		{
			try {
				var i = 0;
				try {
					TestCanRethrowExceptionAsync ().Wait ();
				} catch (DummyException e) {
					Assert.AreEqual ("test", e.Message);
					i++;
				} finally {
					Assert.AreEqual (1, i, "catch hit");
					i++;
				}
				Assert.AreEqual (2, i, "catch and finally hit");
			} finally {
				TestComplete ();
			}
		}
		private Future TestCanRethrowExceptionAsync ()
		{
			Thread.Yield ();
			if (True) {
				try {
					throw new DummyException ("test");
				} catch {
					throw;
				}
			}
			return Future.Fulfilled;
		}
		
		[Test]
		public void TestCanWaitInCatchBlock ()
		{
			var i = 0;
			try {
				throw new Exception ();
			} catch {
				TestCanWaitInCatchBlockAsync ().Wait ();
				i++;
			} finally {
				TestComplete ();
			}
			Assert.AreEqual (1, i);
		}
		private Future TestCanWaitInCatchBlockAsync ()
		{
			return Future.Fulfilled;
		}
	}
}

