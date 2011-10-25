using System;
using System.Collections.Generic;

using Cirrus;
using Cirrus.Gfx;
using Cirrus.Events;

namespace Cirrus.UI {
	
	/// <summary>
	/// A Widget is an object that is both a layer and an event source.
	/// </summary>
	public class Widget : Layer2D,
	
	// --Mouse events:
		IEventSource<MouseMove>,
		IEventSource<MouseDown>,
		IEventSource<MouseUp>,
		IEventSource<Click>
	{
		// Widget children can be anything that extends Layer2D (including Widget),
		// BUT Widget parents can only be things that extend Widget
		new public Widget Parent { get { return base.Parent as Widget; } }
		
		public Widget (Widget parent)
			: base (parent)
		{
			SetupEventHandlers ();
		}
		
		// This is internal to ensure that ONLY a RootWidget can be at the root of the hierarchy
		internal Widget (ICanvas canvas)
			: base (canvas, false)
		{
		}
		
		
		// Mouse events:
		protected virtual Future<TEvent> TranslateMouseEvent<TEvent> (Future<TEvent> parentEvent)
			where TEvent : MouseEvent
		{
			var mouseEvent = parentEvent.Wait ();
			// FIXME: Translate X,Y position of parent into our coord space, etc..
			return mouseEvent;
		}
		
		public virtual Future<MouseMove> OnMouseMove () { return TranslateMouseEvent (Parent.OnMouseMove ()); }
		Future<MouseMove> IEventSource<MouseMove>.GetFuture () { return OnMouseMove (); }

		public virtual Future<MouseDown> OnMouseDown () { return TranslateMouseEvent (Parent.OnMouseDown ()); }
		Future<MouseDown> IEventSource<MouseDown>.GetFuture () { return OnMouseDown (); }
		
		public virtual Future<MouseUp> OnMouseUp () { return TranslateMouseEvent (Parent.OnMouseUp ()); }
		Future<MouseUp> IEventSource<MouseUp>.GetFuture () { return OnMouseUp (); }
		
		public virtual Future<Click> OnClick () { return TranslateMouseEvent (Parent.OnClick ()); }
		Future<Click> IEventSource<Click>.GetFuture () { return OnClick (); }
	}
}

