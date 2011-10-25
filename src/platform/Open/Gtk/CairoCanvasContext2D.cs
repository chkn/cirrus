

using System;
using System.Collections.Generic;
using Cirrus.Gfx;

using Cirrus.Codec.Css;

namespace Cirrus.Open {
	
	public class CairoCanvasContext2D : ICanvasContext2D, IDisposable {
		
		public CairoCanvas Canvas { get; private set; }
		ICanvas ICanvasContext2D.Canvas { get { return Canvas; } }
		
		protected Cairo.Context context;
		
		private CanvasState<Cairo.Pattern,Cairo.Pattern> state;
		private Stack<CanvasState<Cairo.Pattern,Cairo.Pattern>> states;
		
		public CairoCanvasContext2D (CairoCanvas canvas)
		{
			this.Canvas = canvas;
			this.context = new Cairo.Context (canvas.Surface);
			
			this.state = new CanvasState<Cairo.Pattern, Cairo.Pattern> ();
			this.state.stroke = this.state.fill = new Cairo.SolidPattern (new Cairo.Color (0, 0, 0));
			
			this.states = new Stack<CanvasState<Cairo.Pattern,Cairo.Pattern>> ();
		}
		
		public void Save ()
		{
			context.Save ();
			states.Push (state);
		}
		
		public void Restore ()
		{
			context.Restore ();
			state = states.Pop ();
		}

		public void Scale (double x, double y)
		{
			context.Scale (x, y);
		}

		public void Rotate (double angle)
		{
			context.Rotate (angle);
		}
		
		public void Translate (double x, double y)
		{
			context.Translate (x, y);
		}

		public void Transform (double a, double b, double c, double d, double e, double f)
		{
			context.Transform (new Cairo.Matrix (a, b, c, d, e, f));
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
			context.Save ();
			context.Operator = Cairo.Operator.Clear;
			context.Color = new Cairo.Color (0, 0, 0, 0);
			context.Rectangle (x, y, w, h);
			context.Fill ();
			context.Restore ();
		}

		public void FillRect (double x, double y, double w, double h)
		{
			context.NewPath ();
			context.Rectangle (x, y, w, h);
			Fill (false);
			Canvas.Invalidate ((int)x, (int)y, (int)w, (int)h);
		}

		public void StrokeRect (double x, double y, double w, double h)
		{
			context.NewPath ();
			context.Rectangle (x, y, w, h);
			Stroke (false);
			Canvas.Invalidate ((int)x, (int)y, (int)w, (int)h);
		}

		public void BeginPath ()
		{
			context.NewPath ();
		}

		public void ClosePath ()
		{
			context.ClosePath ();
		}

		public void MoveTo (double x, double y)
		{
			context.MoveTo (x, y);
		}

		public void LineTo (double x, double y)
		{
			context.LineTo (x, y);
		}

		public void QuadraticCurveTo (double x1, double y1, double x2, double y2)
		{
			var pt = context.CurrentPoint;
			context.CurveTo (pt.X + 2.0 / 3.0 * (x1 - pt.X),
			                 pt.Y + 2.0 / 3.0 * (y1 - pt.Y),
    		                 x2 + 2.0 / 3.0 * (x1 - x2),
			                 y2 + 2.0 / 3.0 * (y1 - y2),
			                 x2,
			                 y2);
		}

		public void BezierCurveTo (double cp1x, double cp1y, double cp2x, double cp2y, double x, double y)
		{
			context.CurveTo (cp1x, cp1y, cp2x, cp2y, x, y);
		}

		public void ArcTo (double x1, double y1, double x2, double y2, double radius)
		{
			var p0 = context.CurrentPoint;
			var p1 = new Cairo.PointD (x1, y1);
			var p2 = new Cairo.PointD (x2, y2);
			
			if ((p1.X == p0.X && p1.Y == p0.Y)
			 || (p1.X == p2.X && p1.Y == p2.Y)
			 || radius == 0d)
			{
    			context.LineTo (p1);
				return;
			}
			
			throw new NotImplementedException (); // bottom of https://github.com/LearnBoost/node-canvas/blob/master/src/CanvasRenderingContext2d.cc
		}

		public void Rect (double x, double y, double w, double h)
		{
			context.Rectangle (x, y, w, h);
		}

		public void Arc (double x, double y, double radius, double startAngle, double endAngle, bool anticlockwise)
		{
			if (anticlockwise && (Math.PI * 2 != endAngle))
				context.ArcNegative (x, y, radius, startAngle, endAngle);
			else
				context.Arc (x,y, radius, startAngle, endAngle);
		}

		public void Arc (double x, double y, double radius, double startAngle, double endAngle)
		{
			Arc (x, y, radius, startAngle, endAngle, false);
		}
		
		public void Fill ()
		{
			Fill (true);
		}
		
		public void Fill (bool preserve)
		{
			context.Save ();
			context.Source = state.fill;
			//var extents = context.FillExtents ();
			
			if (preserve)
				context.FillPreserve ();
			else
				context.Fill ();
			context.Restore ();
			
			//Canvas.Invalidate ((int)extents.X, (int)extents.Y, (int)extents.Width, (int)extents.Height);
			Canvas.Invalidate ();
		}
		
		public void Stroke ()
		{
			Stroke (true);
		}
		
		public void Stroke (bool preserve)
		{
			context.Save ();
			context.Source = state.stroke;
			//var extents = context.StrokeExtents ();
			
			if (preserve)
				context.StrokePreserve ();
			else
				context.Stroke ();
			context.Restore ();
			
			//Canvas.Invalidate ((int)extents.X, (int)extents.Y, (int)extents.Width, (int)extents.Height);
			Canvas.Invalidate ();
		}

		public void Clip ()
		{
			context.ClipPreserve ();
		}

		public bool IsPointInPath (double x, double y)
		{
			return context.InFill (x, y) || context.InStroke (x, y);
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
			set { state.globalAlpha = value;}
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
					state.stroke = new Cairo.SolidPattern (new Cairo.Color ((double)color.R / 255, (double)color.G / 255, (double)color.B / 255, (double)color.A / 255));
				}
			}
		}

		public object FillStyle {
			get { throw new NotImplementedException (); }
			set {
				Color color;
				if (value is string && CssParser.TryParse ((string)value, out color)) {
					state.fill = new Cairo.SolidPattern (new Cairo.Color ((double)color.R / 255, (double)color.G / 255, (double)color.B / 255, (double)color.A / 255));
				}
			}
		}

		public double LineWidth {
			get { return context.LineWidth; }
			set { context.LineWidth = value;}
		}

		public LineCap LineCap {
			get { return (Cirrus.Gfx.LineCap)context.LineCap; }
			set { context.LineCap = (Cairo.LineCap)value;     }
		}

		public LineJoin LineJoin {
			get { return (Cirrus.Gfx.LineJoin)context.LineJoin; }
			set { context.LineJoin = (Cairo.LineJoin)value;     }
		}

		public double MiterLimit {
			get { return context.MiterLimit; }
			set { context.MiterLimit = value;}
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
		
		public void Dispose ()
		{
			((IDisposable)context).Dispose ();
		}
	}
}

