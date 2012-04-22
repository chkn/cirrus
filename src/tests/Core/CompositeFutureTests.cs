using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;
using Cirrus;
using Cirrus.Test;

namespace Cirrus.Test.Core {
	
	[TestFixture]
	public class CompositeFutureTests : TestsRequireScheduler {
		
		[Test]
		public void TestWaitAnyCompositor ()
		{
			try {
				var f1 = new TimeoutFuture (100);
				var f2 = new TimeoutFuture (400);
				var f3 = new TimeoutFuture (800);
	
				var composite = Future.ForAny (f1, f2, f3);
	
				Assert.That (f1.Status == FutureStatus.Pending &&
				             f2.Status == FutureStatus.Pending &&
				             f3.Status == FutureStatus.Pending, "#1");
				
				composite.Wait ();
				Assert.AreSame (f1, composite.GetFulfilledAndReset ().Single (), "#2a");
				Assert.That (f1.Status == FutureStatus.Fulfilled &&
				             f2.Status == FutureStatus.Pending &&
				             f3.Status == FutureStatus.Pending, "#2b");
				
				Assert.That (composite.Status == FutureStatus.Pending);
				
				composite.Wait ();
				Assert.AreSame (f2, composite.GetFulfilledAndReset ().Single (), "#3a");
				Assert.That (f1.Status == FutureStatus.Fulfilled &&
				             f2.Status == FutureStatus.Fulfilled &&
				             f3.Status == FutureStatus.Pending, "#3b");
				
				Assert.That (composite.Status == FutureStatus.Pending);
				
				composite.Wait ();
				Assert.AreEqual (FutureStatus.Fulfilled, composite.Status);
				Assert.AreSame (f3, composite.GetFulfilledAndReset ().Single (), "#4a");
				Assert.That (f1.Status == FutureStatus.Fulfilled &&
				             f2.Status == FutureStatus.Fulfilled &&
				             f3.Status == FutureStatus.Fulfilled, "#4b");
				
				Assert.AreEqual (FutureStatus.Fulfilled, composite.Status);
			} finally {
				TestComplete ();
			}
		}
		
	}
}

