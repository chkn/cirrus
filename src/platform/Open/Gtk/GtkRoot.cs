using System;
using Gtk;

using Cirrus;
using Cirrus.Gfx;
using Cirrus.UI;
using Cirrus.Events;

namespace Cirrus.Open {
	
	public class GtkRoot : RootWidget {
		
		public Gtk.Window Window { get; private set; }
		
		public GtkCanvas DrawArea { get; private set; }
		
		public GtkRoot (Gtk.Window wnd, GtkCanvas canvas)
			: base (canvas.Canvas)
		{
			this.Window = wnd;
			this.DrawArea = canvas;
			SetupEventHandlers ();
		}
		
		// event sources:
		
		public override Future<BoundsChange> OnBoundsChange ()
		{
			// FIXME: Doesn't pick up window's position changes, only size changes
			var peerChange = Future<ConfigureEventArgs>.FromEvent<ConfigureEventHandler> (
				f => DrawArea.ConfigureEvent += f,
				f => DrawArea.ConfigureEvent -= f
			).Then (args => Bounds.UpdateBounds (args.Event.X, args.Event.Y, args.Event.Width, args.Event.Height));
	
			var userChange = Bounds.On<BoundsChange> ().Then (bc => {
				Window.SetSizeRequest ((int)bc.Bounds.Width, (int)bc.Bounds.Height);
				Window.SetUposition ((int)bc.Bounds.X, (int)bc.Bounds.Y);
				return bc;
			});
			
			return peerChange | userChange;
		}
		
		public override Future<MouseDown> OnMouseDown ()
		{
			throw new NotImplementedException ();
		}
		
		public override Future<MouseMove> OnMouseMove ()
		{
			throw new NotImplementedException ();
		}
		
		public override Future<MouseUp> OnMouseUp ()
		{
			throw new NotImplementedException ();
		}
		
	}
}

