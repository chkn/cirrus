using System;
using System.Linq.Expressions;

namespace Cirrus {
	
	public enum AgentStatus {
		
		// SendMessage will wait until agent is Running before dispatching message.
		Constructing,
		Running,
		Terminated
	}
	
	public abstract class Agent<T> : IDisposable {
		
		public AgentStatus Status { get; protected set; }
		
		public Agent (Expression<Func<T>> construct)
		{
			VerifyConstruct (construct);
		}
		
		public virtual Future SendMessage (Expression<Action<T>> message)
		{
			VerifyMessage (message);
			return VerifyRunning ();
		}
		
		public virtual Future SendMessage (Expression<Func<T,Future>> message)
		{
			VerifyMessage (message);
			return VerifyRunning ();
		}
		
		public virtual Future<V> SendMessage<V> (Expression<Func<T,V>> message)
		{
			VerifyMessage (message);
			VerifyRunning ().Wait ();
			return default (V);
		}
		
		public virtual Future<V> SendMessage<V> (Expression<Func<T,Future<V>>> message)
		{
			VerifyMessage (message);
			VerifyRunning ().Wait ();
			return default (V);
		}
		
		/// <summary>
		/// Verifies that the expression tree representing the construction of the target is fully serializable.
		/// </summary>
		/// <param name="construct">
		/// A <see cref="LambdaExpression"/>
		/// </param>
		protected virtual void VerifyConstruct (LambdaExpression construct)
		{
			
		}
		
		/// <summary>
		/// Verifies the expression tree representing the message to send is fully serializable.
		/// </summary>
		/// <param name="message">
		/// A <see cref="LambdaExpression"/>
		/// </param>
		protected virtual void VerifyMessage (LambdaExpression message)
		{
				
		}
		
		/// <summary>
		/// Returns a Future that will be Fulfilled when this.Status == AgentStatus.Running.
		/// </summary>
		/// <remarks>
		/// The default implementation uses a spin wait to check the value of the Status property.
		/// It may be overridden with a faster implementation depending on the subclass.
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if this Agent has already terminated.</exception>
		/// <returns>
		/// A <see cref="Future"/>
		/// </returns>
		protected virtual Future VerifyRunning ()
		{
			switch (Status) {
			
			case AgentStatus.Terminated:
				throw new InvalidOperationException ("Agent is terminated");
				
			case AgentStatus.Constructing:
				Future.Until (() => Status == AgentStatus.Running).Wait ();
				break;
			}
			
			return Future.Fulfilled;
		}
		
		public virtual void Dispose ()
		{
			Status = AgentStatus.Terminated;	
		}
	}
	

}

