using System;

namespace Cirrus {

	public class ObserverFuture<T> : Future<T>, IObserver<T> {

		protected IDisposable registration;

		public ObserverFuture (IObservable<T> toSubscribe)
		{
			this.registration = toSubscribe.Subscribe (this);
		}

		public virtual void OnNext (T value)
		{
			registration.Dispose ();
			Value = value;
		}

		public virtual void OnError (Exception error)
		{
			registration.Dispose ();
			Exception = error;
		}

		public virtual void OnCompleted ()
		{
			// FIXME: What does this mean? For now, I assume it means the Future is never fulfilled
		}
	}
}

