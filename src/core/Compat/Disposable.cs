using System;

#if !RX
namespace System.Reactive.Disposables {

	internal sealed class DelegateDisposable : IDisposable {
		Action onDispose;
		public DelegateDisposable (Action onDispose)
		{
			this.onDispose = onDispose;
		}
		public void Dispose ()
		{
			onDispose ();
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
