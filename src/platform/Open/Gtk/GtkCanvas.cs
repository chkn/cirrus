using System;
using Gtk;

using Cirrus.Open;

namespace Cirrus.Open {
	
	public class GtkCanvas : DrawingArea {
		
		public CairoCanvas Canvas { get; private set; }
		
		public GtkCanvas (int width, int height)
		{
			DoubleBuffered = true;
			Canvas = new CairoCanvas (width, height);
			Canvas.OnInvalidate += QueueDrawArea;		
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			using (Cairo.Context g = Gdk.CairoHelper.Create (evnt.Window)) {
				
				Canvas.Render (g);
				
			}
			return true;
		}
	}
}

