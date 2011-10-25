using System;
using System.Collections.Generic;

using Cirrus.Gfx;
using Cirrus.Codec.Css;

namespace Cirrus.Mac {
	
	public class CGCanvasContext2D : ICanvasContext2D {
		
		public CGCanvas Canvas { get; private set; }
		ICanvas ICanvasRenderingContext2D.Canvas { get { return Canvas; } }
		
		private IntPtr ctx;
		
		private CanvasState state;
		private Stack<CanvasState> states;
		
		public CGCanvasContext2D (CGCanvas canvas, IntPtr ctx)
		{
			this.Canvas = canvas;
			this.ctx = ctx;
			
			this.state = CanvasState.Default;
			this.states = new Stack<CanvasState> ();
		}
		
		public void Save ()
		{
			CoreGraphics.CGContextSaveGState (ctx);
			states.Push (state);
		}
		
		public void Restore ()
		{
			CoreGraphics.CGContextRestoreGState (ctx);
			state = states.Pop ();
		}

		public void Scale (double x, double y)
		{
			CoreGraphics.CGContextScaleCTM (ctx, (float)x, (float)y);
		}

		public void Rotate (double angle)
		{
			CoreGraphics.CGContextRotateCTM (ctx, (float)angle);
		}
		
		public void Translate (double x, double y)
		{
			CoreGraphics.CGContextTranslateCTM (ctx, (float)x, (float)y);
		}

		public void Transform (double a, double b, double c, double d, double e, double f)
		{
			CoreGraphics.CGContextConcatCTM (ctx, new CoreGraphics.CGAffineTransform((float)a, (float)b, (float)c, (float)d, (float)e, (float)f));
		}

		public ICanvasGradient CreateLinearGradient (double x0, double y0, double x1, double y1)
		{
			throw new NotImplementedException ();
		}

		public ICanvasGradient CreateRadialGradient (double x0, double y0, double r0, double x1, double y1, double r1)
		{
			throw new NotImplementedException ();
		}

		public void ClearRect (double x, double y, double w, double h)
		{
			CoreGraphics.CGContextClearRect (ctx, new CoreGraphics.CGRect ((float)x, (float)y, (float)w, (float)h));
			Canvas.Invalidate ((float)x, (float)y, (float)w, (float)h);
		}

		public void FillRect (double x, double y, double w, double h)
		{
			CoreGraphics.CGContextFillRect (ctx, new CoreGraphics.CGRect ((float)x, (float)y, (float)w, (float)h));
			Canvas.Invalidate ((float)x, (float)y, (float)w, (float)h);
		}

		public void StrokeRect (double x, double y, double w, double h)
		{
			CoreGraphics.CGContextStrokeRect (ctx, new CoreGraphics.CGRect ((float)x, (float)y, (float)w, (float)h));
			Canvas.Invalidate ((float)x, (float)y, (float)w, (float)h);
		}

		public void BeginPath ()
		{
			CoreGraphics.CGContextBeginPath (ctx);
		}

		public void ClosePath ()
		{
			CoreGraphics.CGContextClosePath (ctx);
		}

		public void MoveTo (double x, double y)
		{
			CoreGraphics.CGContextMoveToPoint (ctx, (float)x, (float)y);
		}

		public void LineTo (double x, double y)
		{
			CoreGraphics.CGContextAddLineToPoint (ctx, (float)x, (float)y);
		}

		public void QuadraticCurveTo (double cpx, double cpy, double x, double y)
		{
			CoreGraphics.CGContextAddQuadCurveToPoint (ctx, (float)cpx, (float)cpy, (float)x, (float)y);
		}

		public void BezierCurveTo (double cp1x, double cp1y, double cp2x, double cp2y, double x, double y)
		{
			CoreGraphics.CGContextAddCurveToPoint (ctx, (float)cp1x, (float)cp1y, (float)cp2x, (float)cp2y, (float)x, (float)y);
		}

		public void ArcTo (double x1, double y1, double x2, double y2, double radius)
		{
			CoreGraphics.CGContextAddArcToPoint (ctx, (float)x1, (float)y1, (float)x2, (float)y2, (float)radius);
		}

		public void Rect (double x, double y, double w, double h)
		{
			CoreGraphics.CGContextAddRect (ctx, new CoreGraphics.CGRect ((float)x, (float)y, (float)w, (float)h));
		}

		public void Arc (double x, double y, double radius, double startAngle, double endAngle, bool anticlockwise)
		{
			CoreGraphics.CGContextAddArc (ctx, (float)x, (float)y, (float)radius, (float)startAngle, (float)endAngle, anticlockwise? 0 : 1);
		}

		public void Arc (double x, double y, double radius, double startAngle, double endAngle)
		{
			Arc (x, y, radius, startAngle, endAngle, false);
		}

		public void Fill ()
		{
			CoreGraphics.CGContextFillPath (ctx);
			//FIXME: Actually calc path bounds and just invalidate that
			Canvas.Invalidate ();
		}

		public void Stroke ()
		{
			CoreGraphics.CGContextStrokePath (ctx);
			//FIXME: Actually calc path bounds and just invalidate that
			Canvas.Invalidate ();
		}

		public void Clip ()
		{
			CoreGraphics.CGContextClip (ctx);
		}

		public bool IsPointInPath (double x, double y)
		{
			return CoreGraphics.CGContextPathContainsPoint (ctx, new CoreGraphics.CGPoint ((float)x, (float)y), 3);
		}

		public void FillText (string text, double x, double y, double maxWidth)
		{
			throw new NotImplementedException ();
		}

		public void FillText (string text, double x, double y)
		{
			throw new NotImplementedException ();
		}

		public void StrokeText (string text, double x, double y, double maxWidth)
		{
			throw new NotImplementedException ();
		}

		public void StrokeText (string text, double x, double y)
		{
			throw new NotImplementedException ();
		}

		public double GlobalAlpha {
			get { return state.globalAlpha; }
			set {
				state.globalAlpha = value;
				CoreGraphics.CGContextSetAlpha (ctx, (float)value);
			}
		}

		public string GlobalCompositeOperation {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public object StrokeStyle {
			get { throw new NotImplementedException (); }
			set {
				Color color;
				if (value is string && CssParser.TryParse ((string)value, out color)) {
					CoreGraphics.CGContextSetRGBStrokeColor (ctx, ((float)color.R)/255, ((float)color.G)/255, ((float)color.B)/255, ((float)color.A)/255);
					state.strokeColor = color;
				}
			}
		}

		public object FillStyle {
			get { throw new NotImplementedException (); }
			set {
				Color color;
				if (value is string && CssParser.TryParse ((string)value, out color)) {
					CoreGraphics.CGContextSetRGBFillColor (ctx, ((float)color.R)/255, ((float)color.G)/255, ((float)color.B)/255, ((float)color.A)/255);
					state.fillColor = color;
				}
			}
		}

		public double LineWidth {
			get { return state.lineWidth; }
			set { 
				CoreGraphics.CGContextSetLineWidth (ctx, (float)value);
				state.lineWidth = value;
			}
		}

		public LineCap LineCap {
			get { return state.lineCap; }
			set {
				CoreGraphics.CGContextSetLineCap (ctx, value);
				state.lineCap = value;
			}
		}

		public LineJoin LineJoin {
			get { return state.lineJoin; }
			set {
				CoreGraphics.CGContextSetLineJoin (ctx, value);
				state.lineJoin = value;
			}
		}

		public double MiterLimit {
			get { return state.miterLimit; }
			set {
				CoreGraphics.CGContextSetMiterLimit (ctx, (float)value);
				state.miterLimit = value;
			}
		}

		public double ShadowOffsetX {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public double ShadowOffsetY {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public double ShadowBlur {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string ShadowColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string Font {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string TextAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string TextBaseline {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
	}
}

