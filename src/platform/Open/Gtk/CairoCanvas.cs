using System;
using Cirrus.Gfx;

namespace Cirrus.Open {
	
	public class CairoCanvas : ICanvas, IDisposable {
		
		// args: x,y,w,h
		public event Action<int,int,int,int> OnInvalidate;
		
		public Cairo.Surface Surface { get; set; }
		private int width, height;
		
		public CairoCanvas (int width, int height) {
			
			this.Surface = new Cairo.ImageSurface (Cairo.Format.ARGB32, width, height);
			
			this.width = width;
			this.height = height;
		}
		
		public ICanvasContext2D GetContext2D ()
		{
			return new CairoCanvasContext2D (this);
		}
		
		public int Width {
			get { return width; }
			set { throw new NotImplementedException (); }
		}
		
		public int Height {
			get { return height; }
			set { throw new NotImplementedException (); }
		}
		
		public void Render (Cairo.Context ctx)
		{
			Surface.Show (ctx, 0, 0);
		}
		
		public void Invalidate (int x, int y, int w, int h)
		{
			if (OnInvalidate != null)
				OnInvalidate (x, y, w, h);
		}
		
		public void Invalidate ()
		{
			Invalidate (0, 0, width, height);
		}
		
		public virtual void Dispose ()
		{
			Surface.Dispose ();
		}
	}
}

