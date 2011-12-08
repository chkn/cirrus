using System;
using System.Linq;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;

using System.Reactive.Disposables;

using Cirrus.Util;

#if false
namespace System.Reactive.Linq {

	internal class SimpleObservable<T> : IObservable<T> {

		List<IObserver<T>> observers;
		public Action Retain, Release;

		public void ForEachObserver (Action<IObserver<T>> action)
		{
			lock (observers) {
				foreach (var observer in observers)
					action (observer);
			}
		}

		public IDisposable Subscribe (IObserver<T> observer)
		{
			lock (observers) {
				if (Retain != null)
					Retain ();
				observers.Add (observer);
			}
			return Disposable.Create (() => {
				lock (observers) {
					observers.Remove (observer);
					if (observers.Count == 0 && Release != null)
						Release ();
				}
			});
		}
	}

	public static class Observable {
		
		// Common Cases:
		
		public static IObservable<EventArgs> FromEvent (Action<EventHandler> addEventHandler, Action<EventHandler> removeEventHandler)
		{
			SimpleObservable<EventArgs> observable;
			ToggleReference<SimpleObservable<EventArgs>> tref;
			CreateObservable (out observable, out tref);

			EventHandler handler = null;
            handler = (object s, EventArgs args) => {
				var obs = tref.Target;
				if (obs == null)
					removeEventHandler (handler);
				obs.ForEachObserver (o => o.OnNext (args));
			};

            addEventHandler (handler);
			return observable;
		}

		public static IObservable<TEvent> FromEvent<TEvent> (Action<EventHandler<TEvent>> addEventHandler, Action<EventHandler<TEvent>> removeEventHandler)
            where TEvent : EventArgs
		{
			SimpleObservable<TEvent> observable;
			ToggleReference<SimpleObservable<TEvent>> tref;
			CreateObservable (out observable, out tref);
			
			EventHandler<TEvent> handler = null;
            handler = (object sender, TEvent args) => {
				var obs = tref.Target;
				if (obs == null)
					removeEventHandler (handler);
				obs.ForEachObserver (o => o.OnNext (args));
			};

            addEventHandler (handler);
			return observable;
		}

		public static IObservable<TEvent> FromEvent<TDelegate,TEvent> (Func<Action<TEvent>,TDelegate> conv, Action<TDelegate> addEventHandler, Action<TDelegate> removeEventHandler)
		{
			throw new NotImplementedException ();
		}

		static void CreateObservable<T> (out SimpleObservable<T> observable, out ToggleReference<SimpleObservable<T>> tref)
		{
			observable = new SimpleObservable<T> ();
			tref = new ToggleReference<SimpleObservable<T>> (observable, false);
			observable.Retain =()=> tref.IsReferenced = true;
			observable.Release =()=> tref.IsReferenced = false;
		}

#if !NO_LCG
		
		// Type -> DynamicMethod
		private static Dictionary<Type,WeakReference> event_bridge_cache = new Dictionary<Type,WeakReference> ();

		public static IObservable<TEventArgs> FromEvent<TDel,TEventArgs> (Action<TDel> addEventHandler, Action<TDel> removeEventHandler)
			where TDel : class /* (Delegate) */
		{
			if (addEventHandler == null)
				throw new ArgumentNullException ("addEventHandler");
			if (removeEventHandler == null)
				throw new ArgumentNullException ("removeEventHandler");
					
			WeakReference weakRef = null;
			DynamicMethod handler = null;
			
			if (event_bridge_cache.TryGetValue (typeof (TDel), out weakRef)) {
				handler = weakRef.Target as DynamicMethod;
			}
			
			if (handler == null) {
				
				var invoke = typeof (TDel).GetMethod ("Invoke");
				if (invoke == null)
					throw new ArgumentException ("TDel must be a delegate type"); 
				if (!invoke.ReturnType.Equals (typeof (void)))
					throw new ArgumentException ("TDel must return void");
				
				var invokeParams = invoke.GetParameters ();
				Type [] myParams = new Type [invokeParams.Length + 1];
				myParams [0] = FutureEventHandlerData<TDel>._type;
				for (var i = 0; i < invokeParams.Length; i++)
					myParams [i + 1] = invokeParams [i].ParameterType;
				
				handler = new DynamicMethod ("__feb_" + typeof (TDel).Name, invoke.ReturnType, myParams, typeof (Future).Module);
				var il = handler.GetILGenerator ();
				
				// removeEventHandler (handler)
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldfld, FutureEventHandlerData<TDel>._removeEventHandler);
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldfld, FutureEventHandlerData<TDel>._handler);
				il.Emit (OpCodes.Call, FutureEventHandlerData<TDel>._removeEventHandler_Invoke);
				
				// future.Value = <...>
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldfld, FutureEventHandlerData<TDel>._future);
				il.Emit (OpCodes.Ldarg, invokeParams.Single (p => p.ParameterType.Equals (typeof (T))).Position + 1);
				il.Emit (OpCodes.Call, FutureEventHandlerData<TDel>._future_SetValue);
				
				il.Emit (OpCodes.Ret);
				event_bridge_cache [typeof (TDel)] = new WeakReference (handler);
			}
			
			var data = new FutureEventHandlerData<TDel> (handler, removeEventHandler);
			addEventHandler (data.handler);
			return data.future;
		}
					
		internal class FutureEventHandlerData<TDel> 
			where TDel : class
		{
			public static readonly Type _type = typeof (FutureEventHandlerData<>).MakeGenericType (typeof (T), typeof (TDel));
			
			public Future<T> future;
			public static readonly FieldInfo _future = _type.GetField ("future");
			public static readonly MethodInfo _future_SetValue = typeof (Future<>).MakeGenericType (typeof (T)).GetProperty ("Value").GetSetMethod ();
			
			public TDel handler;
			public static readonly FieldInfo _handler = _type.GetField ("handler");
			
			public Action<TDel> removeEventHandler;
			public static readonly FieldInfo _removeEventHandler = _type.GetField ("removeEventHandler");
			public static readonly MethodInfo _removeEventHandler_Invoke = typeof (Action<>).MakeGenericType (typeof (TDel)).GetMethod ("Invoke");
			
			public FutureEventHandlerData (DynamicMethod handler, Action<TDel> removeEventHandler)
			{
				this.future = new Future<T> ();
				this.handler = handler.CreateDelegate (typeof (TDel), this) as TDel;
				this.removeEventHandler = removeEventHandler;
			}
		}
		
#endif // !NO_LCG
	}
}
#endif //!RX
