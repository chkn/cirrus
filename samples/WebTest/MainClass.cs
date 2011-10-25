using System;
using System.Linq;
using System.Collections.Generic;

using Cirrus;
using Cirrus.Web;

namespace WebTest {
	public static class Foo {
		
		public static void Main ()
		{
			Init ();
			Thread.Current.RunLoop ();
		}
		
		[EntryPoint]
		public static void Init ()
		{
			var pages = new FutureCollection<Http.Response> () {
				
				Http.Request ("http://blog.nirvanai.com"),
				Http.Request ("http://ajaxian.com"),
				Http.Request ("http://go-mono.com/monologue")
			};
			
			pages.ForEach (r => {
			
				Console.WriteLine ("Request to {0} complete:", r.Request.URL);
				Console.WriteLine (r.ResponseText);
				
				return true;
			}).Wait ();
			
			Console.WriteLine ("Done!");
		}
		
	}
	
	public class TestObserver : IObserver<Future>
	{
		public void OnCompleted ()
		{
			Console.WriteLine ("{0} completed!", this);
		}
		
		public void OnError (Exception exception)
		{
			Console.WriteLine ("{0} got error: {1}", this, exception);
		}
		
		public void OnNext (Future f)
		{
			Console.WriteLine ("{0} got next: {1}", this, f);
		}
		
	}
}

