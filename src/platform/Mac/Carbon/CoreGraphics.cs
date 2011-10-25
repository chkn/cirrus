using System;
using System.Runtime.InteropServices;

using Cirrus.Gfx;

namespace Cirrus.Mac {
	
	internal static class CoreGraphics {
		
		#region CGLayer
		
		[DllImport (Carbon.LIB)] public static extern IntPtr CGLayerCreateWithContext (IntPtr context, CGSize size, IntPtr auxiliaryInfo);
		[DllImport (Carbon.LIB)] public static extern CGSize CGLayerGetSize (IntPtr cgLayer);
		[DllImport (Carbon.LIB)] public static extern IntPtr CGLayerGetContext (IntPtr cgLayer);
		[DllImport (Carbon.LIB)] public static extern void CGContextDrawLayerAtPoint (IntPtr context, CGPoint pt, IntPtr layer);
		[DllImport (Carbon.LIB)] public static extern void CGLayerRelease (IntPtr cgLayer);
		
		#endregion
		
		#region Drawing
		
		[DllImport (Carbon.LIB)] public static extern void CGContextSaveGState(IntPtr c);
		[DllImport (Carbon.LIB)] public static extern void CGContextRestoreGState(IntPtr c);
		[DllImport (Carbon.LIB)] public static extern void CGContextScaleCTM(IntPtr c, float sx, float sy);
		[DllImport (Carbon.LIB)] public static extern void CGContextTranslateCTM(IntPtr c, float tx, float ty);
		[DllImport (Carbon.LIB)] public static extern void CGContextRotateCTM(IntPtr c, float angle);
		[DllImport (Carbon.LIB)] public static extern void CGContextConcatCTM(IntPtr c, CGAffineTransform t);
		[DllImport (Carbon.LIB)] public static extern void CGContextSetLineWidth(IntPtr c, float width);
		[DllImport (Carbon.LIB)] public static extern void CGContextRelease(IntPtr c);
		
		[DllImport (Carbon.LIB)] public static extern void CGContextSetLineCap(IntPtr c, LineCap cap);
		[DllImport (Carbon.LIB)] public static extern void CGContextSetLineJoin(IntPtr c, LineJoin join);
		
		[DllImport (Carbon.LIB)] public static extern void CGContextSetMiterLimit(IntPtr c, float limit);
		[DllImport (Carbon.LIB)] public static extern void CGContextSetAlpha(IntPtr c, float alpha);
		[DllImport (Carbon.LIB)] public static extern void CGContextBeginPath(IntPtr c);
		[DllImport (Carbon.LIB)] public static extern void CGContextMoveToPoint(IntPtr c, float x, float y);
		[DllImport (Carbon.LIB)] public static extern void CGContextAddLineToPoint(IntPtr c, float x, float y);
		[DllImport (Carbon.LIB)] public static extern void CGContextAddCurveToPoint(IntPtr c, float cp1x,
   		 float cp1y, float cp2x, float cp2y, float x, float y);
		[DllImport (Carbon.LIB)] public static extern void CGContextAddQuadCurveToPoint(IntPtr c, float cpx,
    	 float cpy, float x, float y);
		[DllImport (Carbon.LIB)] public static extern void CGContextClosePath(IntPtr c);
		[DllImport (Carbon.LIB)] public static extern void CGContextAddRect(IntPtr c, CGRect rect);
		[DllImport (Carbon.LIB)] public static extern void CGContextAddArc(IntPtr c, float x, float y,
    	 float radius, float startAngle, float endAngle, int clockwise);
		[DllImport (Carbon.LIB)] public static extern void CGContextAddArcToPoint(IntPtr c, float x1, float y1,
    	 float x2, float y2, float radius);
		[DllImport (Carbon.LIB)] public static extern bool CGContextPathContainsPoint(IntPtr context, CGPoint point, int mode);
		[DllImport (Carbon.LIB)] public static extern void CGContextStrokePath(IntPtr c);
		[DllImport (Carbon.LIB)] public static extern void CGContextFillPath(IntPtr c);
		[DllImport (Carbon.LIB)] public static extern void CGContextClip(IntPtr c);
		
		[DllImport (Carbon.LIB)] public static extern void CGContextFillRect(IntPtr c, CGRect rect);
		[DllImport (Carbon.LIB)] public static extern void CGContextStrokeRect(IntPtr c, CGRect rect);
		[DllImport (Carbon.LIB)] public static extern void CGContextClearRect(IntPtr c, CGRect rect);
		
		[DllImport (Carbon.LIB)] public static extern void CGContextSetRGBFillColor(IntPtr c, float r, float g, float b, float a);
		[DllImport (Carbon.LIB)] public static extern void CGContextSetRGBStrokeColor(IntPtr c, float r, float g, float b, float a);
		
		[DllImport (Carbon.LIB)] public static extern IntPtr CGColorSpaceCreateDeviceRGB ();
		[DllImport (Carbon.LIB)] public static extern void CGColorSpaceRelease (IntPtr csp);
		[DllImport (Carbon.LIB)] public static extern IntPtr CGColorCreate (IntPtr colorspace, float[] components);
		[DllImport (Carbon.LIB)] public static extern void CGColorRelease (IntPtr color);
		
		#endregion
		
		#region Structs
		
		public struct CGAffineTransform {
			public float a, b, c, d;
			public float tx, ty;
			
			public CGAffineTransform (float a, float b, float c, float d, float tx, float ty)
			{
				this.a = a;
				this.b = b;
				this.c = c;
				this.d = d;
				this.tx = tx;
				this.ty = ty;
			}
		};
		public struct CGPoint {
			public float x;
			public float y;
			
			public CGPoint (float x, float y)
			{
				this.x = x;
				this.y = y;
			}
		};
		public struct CGSize {
			public float width;
			public float height;
	
			public CGSize (float w, float h)
			{
				this.width = w;
				this.height = h;
			}
		};
		public struct CGRect {
			public CGPoint origin;
			public CGSize size;
			
			public CGRect (float x, float y, float w, float h)
			{
				this.origin = new CGPoint (x, y);
				this.size = new CGSize (w, h);
			}
		};
		
		#endregion
		
	}
	
}