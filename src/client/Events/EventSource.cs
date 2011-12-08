using System;
using System.Linq;
using System.Reflection.Emit;
using System.Diagnostics;

using Cirrus;


namespace Cirrus.Events {
	
	public interface IEventSource<TEvent>
	{
		IObservable<TEvent> Observable { get; }
	}
	
	public static class EventSource {
		
		/// <summary>
		/// Get a Future representing the next event from an IEventSource.
		/// </summary>
		/// <example>
		/// obj.OnNext&lt;Click&gt; ().Wait ()
		/// </example>
		/// <param name='source'>
		/// The event source.
		/// </param>
		/// <typeparam name='TEvent'>
		/// The event type.
		/// </typeparam>
		public static Future<TEvent> OnNext<TEvent> (this IEventSource<TEvent> source)
		{
			return source.Observable.Next ();
		}

		/// <summary>
		/// Adds a handler to be called when an event is raised.
		/// </summary>
		/// <param name='source'>
		/// The event source.
		/// </param>
		/// <param name='handler'>
		/// The handler delegate. It will be called every time the event is raised.
		/// It should return True to continue receiving this event, or False to remove itself.
		/// </param>
		/// <typeparam name='TEvent'>
		/// The event type.
		/// </typeparam>
		public static Future OnEvery<TEvent> (this IEventSource<TEvent> source, Func<TEvent,Future<bool>> handler)
		{
			var observer = new ForEachObserver (handler);
			source.Observable.Subscribe (observer);
			return observer.IterationComplete;
		}
	}
}

