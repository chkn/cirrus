using System;

using Cirrus;
using Cirrus.Gfx;
using Cirrus.Events;

namespace Cirrus.UI {
	
	/// <summary>
	/// The root of the view hierarchy.
	/// </summary>
	/// <remarks>
	/// This class requires a platform-specific implementation. A single instance
	/// should be created and returned as Platform.Root.
	/// </remarks>
	public abstract class RootWidget : Widget {
		
		public RootWidget (ICanvas canvas)
			: base (canvas)
		{
		}
		
		// deault implementation for click is based on mouse down, mouse up
		public override Future<Click> OnClick ()
		{
			OnMouseDown ().Wait ();
			var evt = OnMouseUp ().Wait ();
			
			return new Click { X = evt.X, Y = evt.Y };
		}
		
		// These events must be implemented by the platform :
		
		// visual:
		public abstract override Future<BoundsChange> OnBoundsChange ();
		
		// mouse:
		public abstract override Future<MouseMove> OnMouseMove ();
		public abstract override Future<MouseDown> OnMouseDown ();
		public abstract override Future<MouseUp> OnMouseUp ();

	}
}

