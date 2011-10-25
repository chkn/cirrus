using System;
using System.Linq;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;

namespace Cirrus {
	public partial class Future<T> {
		
		// Common Cases:
		
		public static Future<T> FromEvent (Action<EventHandler> addEventHandler, Action<EventHandler> removeEventHandler)
		{
			var future = new Future<T> ();
			
			EventHandler handler = null;
            handler = delegate (object sender, EventArgs args) {
				removeEventHandler (handler);
				future.Value = (T)(object)args;
			};

            addEventHandler (handler);
			return future;
		}
		
		public static Future<T> FromEvent<TEvent> (Action<EventHandler<TEvent>> addEventHandler, Action<EventHandler<TEvent>> removeEventHandler)
            where TEvent : EventArgs, T
		{
			var future = new Future<T> ();
			
			EventHandler<TEvent> handler = null;
            handler = delegate (object sender, TEvent args) {
				removeEventHandler (handler);
				future.Value = args;
			};

            addEventHandler (handler);
			return future;
		}
		
#if !NO_LCG
		
		// Type -> DynamicMethod
		private static Dictionary<Type,WeakReference> event_bridge_cache = new Dictionary<Type,WeakReference> ();
		
		// Ex.. Future<FooEventArgs>.FromEvent (f => SomeObj.OnFoo += f, f => SomeObj.OnFoo -= f);
		public static Future<T> FromEvent<TDel> (Action<TDel> addEventHandler, Action<TDel> removeEventHandler)
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
				if (handler == null)
					event_bridge_cache.Remove (typeof (TDel));
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
				event_bridge_cache.Add (typeof (TDel), new WeakReference (handler));
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

