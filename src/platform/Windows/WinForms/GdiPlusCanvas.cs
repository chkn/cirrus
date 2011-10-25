using System;
using System.Windows.Forms;
using System.Drawing;

using Cirrus.Gfx;

namespace Cirrus.Windows
{
	public class GdiPlusCanvas : ICanvas {
		
		private Control control;
		private BufferedGraphics buffer;
		private BufferedGraphicsContext bufferCtx;
		private int width, height;
		
		public GdiPlusCanvas (Control control)
		{
			this.control = control;
			this.bufferCtx = BufferedGraphicsManager.Current;
			
			Resize (control.Width, control.Height);
		}
		
		public ICanvasContext2D GetContext2D ()
		{
			return new GdiPlusContext2D (this, buffer.Graphics);
		}
		
		// FIXME: What about existing contexts??
		public void Resize (int width, int height)
		{
			this.width = width;
			this.height = height;
			
			bufferCtx.MaximumBuffer = new Size (width + 1, height + 1);

			var oldBuffer = buffer;
			buffer = bufferCtx.Allocate (control.CreateGraphics (), new Rectangle (0, 0, width, height));
			
			if (oldBuffer != null) {
				oldBuffer.Render (buffer.Graphics);
				oldBuffer.Dispose ();
			}
		}
		
		public void Render (Graphics g)
		{
			buffer.Render (g);
		}
		
		public void Invalidate (int x, int y, int w, int h)
		{
			control.Invalidate (new Rectangle (x, y, w, h));
		}
		
		public void Invalidate ()
		{
			control.Invalidate ();
		}
		
		public int Width {
			get { return width;           }
			set { control.Width = value;  }
		}
		
		public int Height {
			get { return height;		  }
			set { control.Height = value; }
		}
	}
}

