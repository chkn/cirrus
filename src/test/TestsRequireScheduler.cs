using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Cirrus.Test {
	
	// Tests that use async methods must inherit from this class and call TestComplete () before they return.
	// Also, ExpectedExceptionAttribute cannot be used.
	public abstract class TestsRequireScheduler {
		private bool test_complete;
		
		[SetUp]
		public void PreTest ()
		{
			test_complete = false;
		}
		
		protected void TestComplete ()
		{
			test_complete = true;
		}
		
		[TearDown]
		// Pump the Cirrus event loop until the test completes
		public void PostTest ()
		{
			try {
				while (!test_complete)
					Cirrus.Thread.Current.RunSingleIteration ();
				
			} catch (AssertionException e) {
				Debug.WriteLine ("Test failed: " + e.Message);
				Debug.WriteLine (e.StackTrace);
				throw;
			}
		}
	}
}

