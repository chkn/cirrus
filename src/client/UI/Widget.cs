using System;
using System.Collections.Generic;

using Cirrus;
using Cirrus.Gfx;
using Cirrus.Events;

namespace Cirrus.UI {
	
	/// <summary>
	/// A Widget is an object that is both a layer and an event source.
	/// </summary>
	public class Widget : Layer2d {

		public Event<PressDown> PressDown { get; private set; }
		public Event<PressMove> PressMove { get; private set; }
		public Event<PressUp>   PressUp   { get; private set; }

		// Widget children can be anything that extends Layer2D (including Widget),
		// BUT Widget parents can only be things that extend Widget
		new public Widget Parent { get { return base.Parent as Widget; } }
		
		public Widget (Widget parent)
			: base (parent)
		{
		}
		
		// This is internal to ensure that ONLY a RootWidget can be at the root of the hierarchy
		internal Widget (ISurface2d nativeSurface)
			: base (nativeSurface, false)
		{
		}
		
		
		// Mouse events:
		protected virtual Future<TEvent> TranslatePressEvent<TEvent> (TEvent parentEvent)
			where TEvent : PressEvent
		{
			//var mouseEvent = parentEvent.Wait ();
			// FIXME: Translate X,Y position of parent into our coord space, etc..
			return parentEvent;
		}

	}
}

