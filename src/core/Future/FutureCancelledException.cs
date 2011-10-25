using System;

namespace Cirrus {
	public class FutureCancelledException : Exception {
		
		public Future CancelledFuture { get; private set; }
		
		public FutureCancelledException (Future f)
			: base ("This Future has been cancelled.")
		{
			CancelledFuture = f;
		}
	}
}

