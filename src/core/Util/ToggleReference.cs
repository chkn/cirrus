using System;

namespace Cirrus.Util {

	public sealed class ToggleReference<T>
		where T : class
	{
		T strongRef;
		WeakReference weakRef;

		public ToggleReference (T obj, bool initiallyOn)
		{
			weakRef = new WeakReference (obj);
			if (initiallyOn)
				strongRef = obj;
		}

		public bool IsReferenced {
			get {
				return strongRef != null;
			}
			set {
				strongRef = value? weakRef.Target as T : null;
			}
		}

		public T Target {
			get {
				return weakRef.Target as T;
			}
		}
	}
}

