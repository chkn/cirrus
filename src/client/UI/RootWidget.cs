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
		
		public RootWidget (ISurface2d nativeSurface)
			: base (nativeSurface)
		{
		}
		
		// deault implementation for click is based on mouse down, mouse up
		/*
		public override Future<Click> OnClick ()
		{
			OnMouseDown ().Wait ();
			var evt = OnMouseUp ().Wait ();
			
			return new Click { X = evt.X, Y = evt.Y };
		}
		*/
	}
}

