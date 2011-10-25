using System;
using Cirrus.Gfx;

namespace Cirrus.Mac {
	public class CGCanvas : ICanvas, IDisposable {
		
		private IntPtr hi_view, cg_layer;
		
		public CGCanvas (IntPtr hiView, IntPtr ctx, float width, float height)
		{
			this.hi_view = hiView;
			this.cg_layer = CoreGraphics.CGLayerCreateWithContext (ctx, new CoreGraphics.CGSize (width, height), IntPtr.Zero);
		}
		
		public ICanvasContext2D GetContext2D ()
		{
			return new CGCanvasContext2D (this, CoreGraphics.CGLayerGetContext (cg_layer));
		}
		
		public int Width {
			get { return (int)CoreGraphics.CGLayerGetSize (cg_layer).width; }
			set { throw new NotImplementedException (); }
		}
		
		public int Height {
			get { return (int)CoreGraphics.CGLayerGetSize (cg_layer).height; }
			set { throw new NotImplementedException (); }
		}
		
		public void Resize (int w, int h)
		{
		}
		
		public void Render(IntPtr targetCtx)
		{
			CoreGraphics.CGContextDrawLayerAtPoint (targetCtx, new CoreGraphics.CGPoint (0, 0), cg_layer);	
		}
		
		public void Invalidate (float x, float y, float w, float h)
		{
			var rect = new CoreGraphics.CGRect (x, y, w, h);
			Carbon.HIViewSetNeedsDisplayInRect (hi_view, ref rect, -1);
		}
		
		public void Invalidate ()
		{
			Carbon.HIViewSetNeedsDisplay (hi_view, -1);
		}
		
		public void Dispose ()
		{
			if (cg_layer != IntPtr.Zero) {
				CoreGraphics.CGLayerRelease (cg_layer);
				cg_layer = IntPtr.Zero;
			}
		}
		
		~CGCanvas ()
		{
			Dispose ();
		}
	}
}

