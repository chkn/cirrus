using System;
using Cirrus;

using System.Linq.Expressions;

namespace AgentTest {
	
	public class MainClass {
	
		public event Action<string> Foo;
		
		public static void Main ()
		{
			var main = new MainClass ();
			Init (main);
			main.Raise ();
			main.Raise ();
			Thread.Current.RunLoop ();
		}
		
		[EntryPoint]
		public static void Init (MainClass main)
		{
			/*
			var agent = new ThreadedAgent<MyAgent> (() => new MyAgent ());
			
			Console.WriteLine ("Main thread: Sending message...");
			var complete = agent.SendMessage (a => a.DoSomething ());
			
			System.Threading.Thread.Sleep (1500);
			Console.WriteLine ("Main thread: Hello!");
			
			complete.Wait ();
			Console.WriteLine ("From main thread: Finished.");
			
			return null;
			*/
			string output = Future<string>.FromEvent<Action<string>> (f => main.Foo += f, f => main.Foo -= f).Wait ();
			Console.WriteLine ("Event happened: {0}", output);
			
		}
		
		public void Raise ()
		{
			Foo ("hello!");
		}
		
	}
	/*
	public class MyAgent {
		
		
		
		public MyAgent ()
		{
			Console.WriteLine ("Other thread: Constructing MyAgent.");
		}
		
		public void DoSomething ()
		{
			Console.WriteLine ("Other thread: Got message.");
			for (int i = 0; i < 5; i++) {

				System.Threading.Thread.Sleep (1000);
				Console.WriteLine (i);
			}
		}
	}
	*/
}

