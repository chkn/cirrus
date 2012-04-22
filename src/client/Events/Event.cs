using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

using Interlocked = System.Threading.Interlocked;

namespace Cirrus.Events {

	public struct Event<T> : IObservable<T> {

		public event Action<T> OnNext;

		public IDisposable Subscribe (IObserver<T> observer)
		{
			var @this = this;
			@this.OnNext += observer.OnNext;
			return Disposable.Create (() => @this.OnNext -= observer.OnNext);
		}

		public void Fire (T data)
		{
			var action = OnNext;
			if (action != null)
				action (data);
		}
	}

}

