using System;
using System.Linq;
using System.IO;
using System.Net;

using Cirrus;
using Cirrus.Web;

namespace FuturesTest {
	public static class MyClass {
		
		
		public static void Main ()
		{
			Go2 (10000);
			Thread.Current.RunLoop ();
		}
		
		public static Future Go (uint timeout)
		{
			var requests = new Future [] {
				DoRequest ("http://www.ajaxian.com", timeout),
				DoRequest ("http://www.mono-project.org", timeout),
				DoRequest ("http://www.google.com", timeout)
			};
			
			var composite = new CompositeFuture (requests, FutureCompositors.WaitAny);
			var requestsRemaining = requests.Length;
			
			do {
				composite.Wait ();
				
				var fulfilled = requests.First (f => f.Status == FutureStatus.Fulfilled) as Future<string>;
				Console.WriteLine (fulfilled.Value);
				
				composite.Reset ();
			} while (--requestsRemaining > 0);
			
			return null;
		}
		
		public static Future Go2 (uint timeout)
		{
			var requests = new FutureCollection<string> {
				DoRequest ("http://www.ajaxian.com", timeout),
				DoRequest ("http://www.mono-project.org", timeout),
				DoRequest ("http://www.google.com", timeout)
			};
			
			foreach (string output in requests)
				Console.WriteLine (output);
			
			return null;
		}
		
		public static Future<string> DoRequest (string url, uint timeoutMsec)
		{
			var http = Http.Request (new Http.RequestArgs { URL = url });
			var timeout = Future.FromNow (timeoutMsec);
			
			Future.ForAny (http, timeout).Wait ();
			
			if (http.Status == FutureStatus.Fulfilled)
				return "Got response from " + url + ": " + http.Value.ResponseText.Substring (0, 25);
			else
				return "Request timedout for " + url;
		}
	}
}

