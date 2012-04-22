using System;
using System.Threading;
using System.Collections.Generic;

#if !RX
namespace System.Reactive.Disposables {

	internal sealed class DelegateDisposable : IDisposable {

		Action dispose;

		public DelegateDisposable (Action dispose)
		{
			this.dispose = dispose;
		}
		public void Dispose ()
		{
			var action = Interlocked.Exchange (ref dispose, null);
			if (action != null)
				action ();
		}
	}

	public static class Disposable {

		public static IDisposable Create (Action dispose)
		{
			return new DelegateDisposable (dispose);
		}
	}
}
#endif