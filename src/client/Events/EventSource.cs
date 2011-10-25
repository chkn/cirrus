using System;
using System.Linq;
using System.Reflection.Emit;
using System.Diagnostics;

using Cirrus;


namespace Cirrus.Events {
	
	public interface IEventSource<TEvent> 
		where TEvent : Event
	{
		Future<TEvent> GetFuture ();
	}
	
	public static class EventSource {
		
		/// <summary>
		/// Get a Future for an event.
		/// </summary>
		/// <example>
		/// obj.On&lt;Click&gt; ().Wait ()
		/// </example>
		/// <param name='source'>
		/// The event source.
		/// </param>
		/// <typeparam name='TEvent'>
		/// The event type.
		/// </typeparam>
		public static Future<TEvent> On<TEvent> (this IEventSource<TEvent> source)
			where TEvent : Event
		{
#if DEBUG
			var evt = source.GetFuture ().Wait ();
			evt.Log ();
			return evt;
#else
			return source.GetFuture ();
#endif
		}
		
		public static void OnEvery<TEvent> (this IEventSource<TEvent> source, Action<TEvent> handler)
			where TEvent : Event
		{
			while (true) {
				var evt = source.GetFuture ().Wait ();
				evt.Log ();
				handler (evt);
			}
		}
		
		// Extra convenience for exposing events in Cirrus as Action<TEvent>
		
		public static void Fire<TEvent> (this Action<TEvent> handler, TEvent e)
		{
			var h = handler;
			if (h != null)
				h (e);
		}
		
		public static Future<TEvent> ToFuture<TEvent> (this Action<TEvent> evt)
		{
			return Future<TEvent>.FromEvent<Action<TEvent>> (f => evt += f, f => evt -= f);
		}
	}
	
	public class Event {
		
		[Conditional ("DEBUG")]
		public void Log ()
		{
#if DEBUG
			Console.WriteLine (this);
#endif
		}
	}
}

