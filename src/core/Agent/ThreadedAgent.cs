using System;
using System.Linq.Expressions;

namespace Cirrus {
	
	public class ThreadedAgent<T> : Agent<T> {
		
		private T receiver;
		private Cirrus.Thread run_loop;

		public ThreadedAgent (Expression<Func<T>> construct) : base (construct)
		{	
			this.run_loop = new Thread ();
			
			var physical = new System.Threading.Thread (RunAgent);
			physical.Start (construct.Compile ());
			
			Status = AgentStatus.Running;
		}
		
		private void RunAgent (object constructDel)
		{
			Cirrus.Thread.current = run_loop;
			
			// Even if messages get enqueued here,
			//  we don't start the run loop until after the constructor finishes.
			
			var constructor = (Func<T>) constructDel;
			receiver = constructor ();
			
			do {
				run_loop.RunLoop ();
				
			} while (Status == AgentStatus.Running);
		}
		
		public override Future SendMessage (Expression<Action<T>> message)
		{
			base.SendMessage (message).Wait ();
			
			var invocation = message.Compile ();
			var proxy = new ProxyFuture (() => invocation (receiver));
			
			proxy.Schedule (run_loop);
			proxy.Wait ();
			
			return Future.Fulfilled;
		}
		
		public override Future SendMessage (Expression<Func<T,Future>> message)
		{
			base.SendMessage (message).Wait ();
			
			return Future.Fulfilled;
		}
		
		public override Future<V> SendMessage<V> (Expression<Func<T, V>> message)
		{
			base.SendMessage (message).Wait ();
			
			return default (V);
		}
		
		public override Future<V> SendMessage<V> (Expression<Func<T,Future<V>>> message)
		{
			base.SendMessage (message).Wait ();
			
			return default (V);
		}
	}
	
}

