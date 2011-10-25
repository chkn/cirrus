using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Cirrus.Gfx;
using Cirrus.Codec.Css;


namespace Cirrus.Windows
{
	public class GdiPlusContext2D : ICanvasContext2D {
		
		private static readonly Brush transparent_brush = new SolidBrush (System.Drawing.Color.Transparent);
		
		public GdiPlusCanvas Canvas { get; private set; }
		ICanvas ICanvasContext2D.Canvas { get { return Canvas; } }
		
		private Graphics g;
		
		private CanvasState<Brush,Pen> state;
		private Stack<CanvasState<Brush,Pen>> states;
		private Stack<GraphicsState> gstates;
		
		private GraphicsPath current_path;
		private PointF? start_point, current_point;
		
		public GdiPlusContext2D (GdiPlusCanvas canvas, Graphics g)
		{
			this.Canvas = canvas;
			this.g = g;
			
			// Init state
			g.SmoothingMode = SmoothingMode.AntiAlias;
			
			this.state.fill = new SolidBrush (System.Drawing.Color.Black);
			
			this.state.stroke = new Pen (System.Drawing.Color.Black, 1);
			this.state.stroke.SetLineCap (System.Drawing.Drawing2D.LineCap.Flat, System.Drawing.Drawing2D.LineCap.Flat, DashCap.Flat);
			this.state.stroke.LineJoin = System.Drawing.Drawing2D.LineJoin.Miter;
			this.state.stroke.MiterLimit = 10;
			
			this.states = new Stack<CanvasState<Brush,Pen>> ();
			this.gstates = new Stack<GraphicsState> ();
			this.current_path = new GraphicsPath ();
		}
		
		public void Save ()
		{
			states.Push (state);
			gstates.Push (g.Save ());
		}
		
		public void Restore ()
		{
			g.Restore (gstates.Pop ());
			state = states.Pop ();
		}

		public void Scale (double x, double y)
		{
			g.ScaleTransform ((float)x, (float)y);
		}

		public void Rotate (double angle)
		{
			g.RotateTransform ((float)(angle * 180 / Math.PI));
		}
		
		public void Translate (double x, double y)
		{
			g.TranslateTransform ((float)x, (float)y);
		}

		public void Transform (double a, double b, double c, double d, double e, double f)
		{
			g.MultiplyTransform (new Matrix ((float)a, (float)b, (float)c, (float)d, (float)e, (float)f));
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
			var gs = g.Save ();
			g.CompositingMode = CompositingMode.SourceCopy;
			g.FillRectangle (transparent_brush, new Rectangle ((int)x, (int)y, (int)w, (int)h));
			g.Restore (gs);
			Canvas.Invalidate ((int)x, (int)y, (int)w, (int)h);
		}

		public void FillRect (double x, double y, double w, double h)
		{
			g.FillRectangle (state.fill, new Rectangle ((int)x, (int)y, (int)w, (int)h));
			Canvas.Invalidate ((int)x, (int)y, (int)w, (int)h);
		}

		public void StrokeRect (double x, double y, double w, double h)
		{
			g.DrawRectangle (state.stroke, new Rectangle ((int)x, (int)y, (int)w, (int)h));
			Canvas.Invalidate ((int)x, (int)y, (int)w, (int)h);
		}

		public void BeginPath ()
		{
			current_path.Reset ();
			current_path.StartFigure ();
			start_point = current_point;
		}

		public void ClosePath ()
		{
			if (current_point.HasValue) {
				current_path.CloseFigure ();
				current_point = start_point;
				start_point = null;
			}
		}

		public void MoveTo (double x, double y)
		{
				var points = new PointF [] { new PointF ((float)x, (float)y) };
				//using (var xform = g.Transform)
				//	xform.TransformPoints (points);
				current_point = points [0];
				if (!start_point.HasValue)
					start_point = current_point;
		}

		public void LineTo (double x, double y)
		{
			if (!EnsureSubpath (x, y))
				return;
			
			var points = new PointF [] { new PointF ((float)x, (float)y) };
			//using (var xform = g.Transform)
			//	xform.TransformPoints (points);
			
			current_path.AddLine (current_point.Value, points [0]);
			current_point = points [0];
		}

		public void QuadraticCurveTo (double x1, double y1, double x2, double y2)
		{
			if (!EnsureSubpath (x1, y1))
				return;
			
			var pt = current_point.Value;
			var new_pt = new PointF ((float)x2, (float)y2);
			current_path.AddCurve (new PointF [] {
				current_point.Value,
				new PointF (pt.X + 2.0f / 3.0f * ((float)x1 - pt.X), pt.Y + 2.0f / 3.0f * ((float)y1 - pt.Y)),
    		    new PointF ((float)x2 + 2.0f / 3.0f * ((float)x1 - (float)x2), (float)y2 + 2.0f / 3.0f * ((float)y1 - (float)y2)),
			    new_pt
			});
			current_point = new_pt;
		}

		public void BezierCurveTo (double cp1x, double cp1y, double cp2x, double cp2y, double x, double y)
		{
			if (!EnsureSubpath (cp1x, cp1y))
				return;
			
			var new_pt = new PointF ((float)x, (float)y);
			current_path.AddBezier (current_point.Value,
			                        new PointF ((float)cp1x, (float)cp1y),
			                        new PointF ((float)cp2x, (float)cp2y),
			                        new_pt);
			current_point = new_pt;
		}

		public void ArcTo (double x1, double y1, double x2, double y2, double radius)
		{
			throw new NotImplementedException ();
		}

		public void Rect (double x, double y, double w, double h)
		{
			current_point = start_point = new PointF ((float)x, (float)y);
			current_path.AddRectangle (new RectangleF ((float)x, (float)y, (float)w, (float)h));
			ClosePath ();
		}

		public void Arc (double x, double y, double radius, double startAngle, double endAngle, bool anticlockwise)
		{
			current_path.AddArc ((float)(x - radius), (float)(y - radius), (float)radius * 2, (float)radius * 2, (float)(anticlockwise? endAngle : startAngle), (float)(anticlockwise? startAngle : endAngle));
		}

		public void Arc (double x, double y, double radius, double startAngle, double endAngle)
		{
			throw new NotImplementedException ();
		}
		
		public void Fill ()
		{
			//var xform = g.Transform;
			//g.ResetTransform ();
			g.FillPath (state.fill, current_path);
			//g.Transform = xform;
			Canvas.Invalidate ();
		}
		
		public void Stroke ()
		{
			//var xform = g.Transform;
			//g.ResetTransform ();
			g.DrawPath (state.stroke, current_path);
			//g.Transform = xform;
			Canvas.Invalidate ();
		}

		public void Clip ()
		{
			g.SetClip (current_path, CombineMode.Intersect);
		}

		public bool IsPointInPath (double x, double y)
		{
			throw new NotImplementedException ();
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
				Cirrus.Gfx.Color color;
				if (value is string && CssParser.TryParse ((string)value, out color)) {
					state.stroke = (Pen)state.stroke.Clone ();
					state.stroke.Color = System.Drawing.Color.FromArgb ((int)color.argb32);
				}
			}
		}

		public object FillStyle {
			get { throw new NotImplementedException (); }
			set {
				Cirrus.Gfx.Color color;
				if (value is string && CssParser.TryParse ((string)value, out color)) {
					state.fill = new SolidBrush (System.Drawing.Color.FromArgb ((int)color.argb32));
				}
			}
		}

		public double LineWidth {
			get { return state.stroke.Width;         }
			set { state.stroke.Width = (float)value; }
		}

		public Cirrus.Gfx.LineCap LineCap {
			get {
				switch (state.stroke.StartCap) {
					
				case System.Drawing.Drawing2D.LineCap.Round:
					return Cirrus.Gfx.LineCap.Round;
					
				case System.Drawing.Drawing2D.LineCap.Square:
					return Cirrus.Gfx.LineCap.Square;
				}
				return Cirrus.Gfx.LineCap.Butt;
			}
			set {
				System.Drawing.Drawing2D.LineCap gdiCap = System.Drawing.Drawing2D.LineCap.Flat;
				
				switch (value) {
					
				case Cirrus.Gfx.LineCap.Round:
					gdiCap = System.Drawing.Drawing2D.LineCap.Round;
					break;
					
				case Cirrus.Gfx.LineCap.Square:
					gdiCap = System.Drawing.Drawing2D.LineCap.Square;
					break;
				}
				state.stroke.StartCap = gdiCap;
				state.stroke.EndCap = gdiCap;
			}
		}

		public Cirrus.Gfx.LineJoin LineJoin {
			get {
				switch (state.stroke.LineJoin) {
					
				case System.Drawing.Drawing2D.LineJoin.Miter:
					return Cirrus.Gfx.LineJoin.Miter;
					
				case System.Drawing.Drawing2D.LineJoin.Round:
					return Cirrus.Gfx.LineJoin.Round;
				}
				return Cirrus.Gfx.LineJoin.Bevel;
			}
			set {
				System.Drawing.Drawing2D.LineJoin gdiJoin = System.Drawing.Drawing2D.LineJoin.Bevel;
				
				switch (value) {
					
				case Cirrus.Gfx.LineJoin.Miter:
					gdiJoin = System.Drawing.Drawing2D.LineJoin.Miter;
					break;
					
				case Cirrus.Gfx.LineJoin.Round:
					gdiJoin = System.Drawing.Drawing2D.LineJoin.Round;
					break;
				}
				state.stroke.LineJoin = gdiJoin;
			}
		}

		public double MiterLimit {
			get { return state.stroke.MiterLimit;         }
			set { state.stroke.MiterLimit = (float)value; }
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
			
		private bool EnsureSubpath (double x, double y)
		{
			if (!current_point.HasValue) {
				MoveTo (x, y);
				return false;
			} 
			if (!start_point.HasValue) {
				start_point = current_point;
			}
			return true;
		}
	}
}

