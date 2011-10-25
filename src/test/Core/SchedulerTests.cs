using System;
using NUnit.Framework;

using Cirrus.Test;

namespace Cirrus.Test.Core {

	[TestFixture]
	public class SchedulerTests : TestsRequireScheduler
	{
	
		
		[Test]
		[Ignore]
		public void TestAllScheduledFibersRun ()
		{
			throw new NotImplementedException ();
			TestComplete ();
		}
		private Future TestAllScheduledFibersRunAsync ()
		{
			return null;
		}
		
		[Test]
		public void TestTimeoutFutureSleepTime ()
		{
			var dur = TestTimeoutFutureSleepTimeAsync ().Wait ().TotalMilliseconds;
			Assert.That ((dur >= 500) && (dur < 550), "#1");
			TestComplete ();
		}
		private Future<TimeSpan> TestTimeoutFutureSleepTimeAsync ()
		{
			var start = DateTime.Now;
			Future.MillisecondsFromNow (500).Wait ();
			return DateTime.Now.Subtract (start);
		}
		
	}
}

