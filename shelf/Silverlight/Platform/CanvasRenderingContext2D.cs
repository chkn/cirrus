using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cirrus.Codec.Css.Parsers;

namespace Cirrus.Silverlight {
	
#if WEB
	using System.Windows.Browser;
    public class CanvasRenderingContext2D
#else	
	public class CanvasRenderingContext2D : Cirrus.Gfx.ICanvasRenderingContext2D
#endif
    {
		private CirrusCanvas _cirrusCanvas;
		
        private Canvas _root;
        private Canvas _canvas;

#if WPF
        private RenderTargetBitmap _raster;
#else
		private WriteableBitmap _raster;
#endif

		private Image _rasterImage;
		
        private Brush _fill;
        private Brush _stroke;
        private PathGeometry _pathGeometry;
        private bool _hasSubPaths;
        private TransformGroup _transformGroup = new TransformGroup();
        private Stack<DrawingState> _drawingStates = new Stack<DrawingState>();
		
        public CanvasRenderingContext2D(CirrusCanvas canvas)
        {
            // Initialize
			_cirrusCanvas = canvas;
            _root = canvas.canvas;
			
#if WPF
            //var m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
            _raster = new RenderTargetBitmap((int)_root.Width, (int)_root.Height, /*m.M11 */ 96,/* m.M22 */ 96, PixelFormats.Pbgra32);
#else
            _raster = new WriteableBitmap((int)_root.Width, (int)_root.Height);
#endif
			_rasterImage = new Image() { Source = _raster };

			_canvas = new Canvas { Width = _root.Width, Height = _root.Height, RenderTransform = _transformGroup };
			_root.Children.Add(_canvas);
			
			
            GlobalAlpha = 1.0;
            FillStyle = "#000000";
            StrokeStyle = "#000000";
            LineWidth = 1;
            LineCap = "butt";
            LineJoin = "miter";
            MiterLimit = 10;
            BeginPath();

            // Handle size change by updating children
           _root.SizeChanged += (sender, e) =>
            {
                _root.Width = e.NewSize.Width;
                _root.Height = e.NewSize.Height;
                foreach (var child in _root.Children.OfType<FrameworkElement>())
                {
                    child.Width = _root.Width;
                    child.Height = _root.Height;
                }
            };
            
        }
		
		private void Flatten()
		{
#if FLATTEN
            _root.Arrange(new Rect(0, 0, _root.Width, _root.Height));
			foreach (var child in _root.Children.OfType<UIElement>())
            {
                if (child != _rasterImage) {
#if WPF
                    _raster.Render(child);
#else
					_raster.Render(child, null);
                    _raster.Invalidate();
#endif
                }
            }
			_root.Children.Clear();
			_root.Children.Add(_rasterImage);
#endif
			_canvas = new System.Windows.Controls.Canvas { Width = _root.Width, Height = _root.Height, RenderTransform = _transformGroup };
            _root.Children.Add(_canvas);
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "canvas")]
		public CirrusCanvas Canvas {
#else
		public Cirrus.Gfx.ICanvas Canvas {
#endif
			get {
				return _cirrusCanvas;	
			}
		}

#if WEB
		[ScriptableMember (ScriptAlias = "save")]
#endif
        public void Save()
        {
            // Save state on stack
            _drawingStates.Push(new DrawingState
            {
                strokeStyle = StrokeStyle,
                fillStyle = FillStyle,
                globalAlpha = GlobalAlpha,
                lineWidth = LineWidth,
                lineCap = LineCap,
                lineJoin = LineJoin,
                miterLimit = MiterLimit,
                transformGroup = _transformGroup,
            });
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "restore")]
#endif
        public void Restore()
        {
            // Restore state from stack
            if (_drawingStates.Any())
            {
                var drawingState = _drawingStates.Pop();
                StrokeStyle = drawingState.strokeStyle;
                FillStyle = drawingState.fillStyle;
                GlobalAlpha = drawingState.globalAlpha;
                LineWidth = drawingState.lineWidth;
                LineCap = drawingState.lineCap;
                LineJoin = drawingState.lineJoin;
                MiterLimit = drawingState.miterLimit;

                // Create a new Canvas for the transform
                _transformGroup = drawingState.transformGroup;
				Flatten();
            }
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "scale")]
#endif
        public void Scale(double x, double y)
        {
            // Create new Canvas for the new combined transform
            _transformGroup = Utility.CloneTransformGroup(_transformGroup);
            _transformGroup.Children.Insert(0, new ScaleTransform { ScaleX = x, ScaleY = y });
            Flatten();
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "rotate")]
#endif
        public void Rotate(double angle)
        {
            // Create new Canvas for the new combined transform
            _transformGroup = Utility.CloneTransformGroup(_transformGroup);
            _transformGroup.Children.Insert(0, new RotateTransform { Angle = (angle / (2 * Math.PI)) * 360 });
            Flatten();
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "translate")]
#endif
        public void Translate(double x, double y)
        {
            // Create new Canvas for the new combined transform
            _transformGroup = Utility.CloneTransformGroup(_transformGroup);
            _transformGroup.Children.Insert(0, new TranslateTransform { X = x, Y = y });
            Flatten();
        }
	
#if WEB
		[ScriptableMember (ScriptAlias = "transform")]
#endif
		public void Transform(double a, double b, double c, double d, double e, double f)
		{
			throw new NotImplementedException ();	
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "globalAlpha")]
#endif
        public double GlobalAlpha
        {
            get { return _globalAlpha; }
            set
            {
                if ((0.0 <= value) && (value <= 1.0))
                {
                    _globalAlpha = value;
                }
            }
        }
        private double _globalAlpha;
		
#if WEB
		[ScriptableMember (ScriptAlias = "globalCompositeOperation")]
#endif
		public string GlobalCompositeOperation
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "strokeStyle")]
#endif
        public object StrokeStyle
        {
            get { return _strokeStyle; }
            set
            {
                var stringValue = value as string;
                var gradientValue = value as CanvasGradient;
                var patternValue = value as CanvasPattern;
                if (null != stringValue)
                {
                    // Parse string style
					byte r, g, b, a;
                    if (CssColor.TryParse (stringValue, out r, out g, out b, out a))
                    {
                        _stroke = new SolidColorBrush (Color.FromArgb (a, r, g, b));
                        _strokeStyle = value;
                    }
                }
                else if (null != gradientValue)
                {
                    // Apply gradient style
                    _stroke = gradientValue.GradientBrush;
                    _strokeStyle = value;
                }
                else if (null != patternValue)
                {
                    // Apply pattern style
                    _stroke = patternValue.ImageBrush;
                    _strokeStyle = value;
                }
            }
        }
        private object _strokeStyle;
			
#if WEB
		[ScriptableMember (ScriptAlias = "fillStyle")]
#endif
        public object FillStyle
        {
            get { return _fillStyle; }
            set
            {
                var stringValue = value as string;
                var gradientValue = value as CanvasGradient;
                var patternValue = value as CanvasPattern;
                if (null != stringValue)
                {
                    // Parse string style
					byte r, g, b, a;
                    if (CssColor.TryParse (stringValue, out r, out g, out b, out a))
                    {
                        _fill = new SolidColorBrush (Color.FromArgb (a, r, g, b));
                        _fillStyle = value;
                    }
                }
                else if (null != gradientValue)
                {
                    // Apply gradient style
                    _fill = gradientValue.GradientBrush;
                    _fillStyle = value;
                }
                else if (null != patternValue)
                {
                    // Apply pattern style
                    _fill = patternValue.ImageBrush;
                    _fillStyle = value;
                }
            }
        }
        private object _fillStyle;

#if WEB
		[ScriptableMember (ScriptAlias = "createLinearGradient")]
		public CanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1)
#else
		public Cirrus.Gfx.ICanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1)
#endif
        {
            // Translate coordinates
            var brush = new LinearGradientBrush { StartPoint = new Point(x0, y0), EndPoint = new Point(x1, y1), MappingMode = BrushMappingMode.Absolute };
            return new CanvasGradient { GradientBrush = brush };
        }

#if WEB
        [ScriptableMember (ScriptAlias = "createRadialGradient")]
		public CanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1)
#else
		public Cirrus.Gfx.ICanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1)
#endif
        {
            // Translate coordinates
            var brush = new RadialGradientBrush { GradientOrigin = new Point(x0, y0), Center = new Point(x1, y1), RadiusX = r1, RadiusY = r1, MappingMode = BrushMappingMode.Absolute };
            var rMin = Math.Min(r0, r1);
            var rMax = Math.Max(r0, r1);
            return new CanvasGradient { GradientBrush = brush, OffsetMinimum = rMin / rMax, OffsetMultiplier = (rMax - rMin) / rMax };
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "createPattern")]
		public CanvasPattern CreatePattern(ScriptObject image, string repetition)
#else
		public Cirrus.Gfx.ICanvasPattern CreatePattern (Cirrus.Gfx.Image image, string repetition)
#endif
        
        {
            var brush = new ImageBrush { ImageSource = Utility.DomImageSource(image), Stretch = Stretch.None };
            return new CanvasPattern { ImageBrush = brush };
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "lineWidth")]
#endif
        public double LineWidth
        {
            get { return _lineWidth; }
            set
            {
                if ((0.0 < value) && !double.IsInfinity(value))
                {
                    _lineWidth = value;
                }
            }
        }
        private double _lineWidth;
			
#if WEB
		[ScriptableMember (ScriptAlias = "lineCap")]
#endif
        public string LineCap
        {
            get { return _lineCap.ToString(); }
            set
            {
                try
                {
                    _lineCap = (CapTypes)Enum.Parse(typeof(CapTypes), value, true);
                }
                catch (ArgumentException)
                {
                    // Ignore bogus value
                }
            }
        }
        private CapTypes _lineCap;
			
#if WEB
		[ScriptableMember (ScriptAlias = "lineJoin")]
#endif
        public string LineJoin
        {
            get { return _lineJoin.ToString(); }
            set
            {
                try
                {
                    _lineJoin = (JoinTypes)Enum.Parse(typeof(JoinTypes), value, true);
                }
                catch (ArgumentException)
                {
                    // Ignore bogus value
                }
            }
        }
        private JoinTypes _lineJoin;
			
#if WEB
		[ScriptableMember (ScriptAlias = "miterLimit")]
#endif
        public double MiterLimit
        {
            get { return _miterLimit; }
            set
            {
                if ((0.0 < value) && !double.IsInfinity(value))
                {
                    _miterLimit = value;
                }
            }
        }
        private double _miterLimit;
		
#if WEB
		[ScriptableMember (ScriptAlias = "shadowOffsetX")]
#endif
		public double ShadowOffsetX {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "shadowOffsetY")]
#endif
		public double ShadowOffsetY { 
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "shadowBlur")]
#endif
		public double ShadowBlur {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); } 
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "shadowColor")]
#endif
		public string ShadowColor {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "clearRect")]
#endif
        public void ClearRect(double x, double y, double w, double h)
        {
            if ((0 < w) && (0 < h))
            {
                if ((x <= 0) && (y <= 0) && (_root.Width <= w) && (_root.Height <= h))
                {
                    // Optimize common scenario of clearing the entire canvas
                    _root.Children.Clear();
					_raster.Clear();
					
					// Create a new canvas to draw on
                	_canvas = new System.Windows.Controls.Canvas { Width = _root.Width, Height = _root.Height, RenderTransform = _transformGroup };
                	_root.Children.Add(_canvas);
                }
                else
                {
#if FLATTEN && !WPF
					Flatten();
					_raster.FillRectangle((int)x, (int)y, (int)(x+w), (int)(y+h), 0);
					_raster.Invalidate();
#else
					// Apply the necessary exclusive clip to all children
                    foreach (var canvas in _root.Children.OfType<Canvas>())
                    {
                        var group = new GeometryGroup();
                        group.Children.Add(new RectangleGeometry { Rect = new Rect(0, 0, _root.Width, _root.Height) });
                        group.Children.Add(new RectangleGeometry { Rect = new Rect(x, y, w, h) });
                        canvas.Clip = group;
                    }
					// Create a new canvas to draw on
                    Flatten();
#endif
                }

            }
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "fillRect")]
#endif
        public void FillRect(double x, double y, double w, double h)
        {
            if ((0 < w) && (0 < h))
            {
                var rect = new Rectangle { Width = w, Height = h, Fill = Utility.CloneAndMapGradientBrush(_fill, x, y), Opacity = GlobalAlpha };
                System.Windows.Controls.Canvas.SetLeft(rect, x);
                System.Windows.Controls.Canvas.SetTop(rect, y);
                _canvas.Children.Add(rect);
            }
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "strokeRect")]
#endif
        public void StrokeRect(double x, double y, double w, double h)
        {
            var rect = new Rectangle { Width = w, Height = h, Stroke = Utility.CloneAndMapGradientBrush(_stroke, x, y), StrokeThickness = LineWidth, StrokeLineJoin = JoinTypeToPenLineJoin(_lineJoin), StrokeMiterLimit = _miterLimit, Opacity = GlobalAlpha };
            System.Windows.Controls.Canvas.SetLeft(rect, x);
            System.Windows.Controls.Canvas.SetTop(rect, y);
            _canvas.Children.Add(rect);
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "beginPath")]
#endif
        public void BeginPath()
        {
            _pathGeometry = new PathGeometry();
            _pathGeometry.Figures.Add(new PathFigure { StartPoint = new Point() });
            _hasSubPaths = false;
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "closePath")]
#endif
        public void ClosePath()
        {
            if (_hasSubPaths)
            {
                var figure = _pathGeometry.Figures.Last();
                figure.IsClosed = true;
                figure = new PathFigure { StartPoint = figure.StartPoint };
            }
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "moveTo")]
#endif
        public void MoveTo(double x, double y)
        {
            var figure = new PathFigure { StartPoint = new Point(x, y) };
            _pathGeometry.Figures.Add(figure);
            _hasSubPaths = true;
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "lineTo")]
#endif
        public void LineTo(double x, double y)
        {
            if (!_hasSubPaths)
            {
                MoveTo(x, y);
            }
            var figure = _pathGeometry.Figures.Last();
            figure.Segments.Add(new LineSegment { Point = new Point(x, y) });
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "quadraticCurveTo")]
#endif
        public void QuadraticCurveTo(double cpx, double cpy, double x, double y)
        {
            if (!_hasSubPaths)
            {
                MoveTo(x, y);
            }
            var figure = _pathGeometry.Figures.Last();
            figure.Segments.Add(new QuadraticBezierSegment { Point1 = new Point(cpx, cpy), Point2 = new Point(x, y) });
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "bezierCurveTo")]
#endif
        public void BezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y)
        {
            if (!_hasSubPaths)
            {
                MoveTo(x, y);
            }
            var figure = _pathGeometry.Figures.Last();
            figure.Segments.Add(new BezierSegment { Point1 = new Point(cp1x, cp1y), Point2 = new Point(cp2x, cp2y), Point3 = new Point(x, y) });
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "arc")]
#endif
		public void Arc(double x, double y, double radius, double startAngle, double endAngle)
		{
			Arc (x, y, radius, startAngle, endAngle);		
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "arc")]
#endif
        public void Arc(double x, double y, double radius, double startAngle, double endAngle, bool anticlockwise)
        {
            var figure = _pathGeometry.Figures.Last();
            var start = new Point(x + (radius * Math.Cos(startAngle)), y + (radius * Math.Sin(startAngle)));
            if (_hasSubPaths)
            {
                // Draw line from last point of path to start of arc
                figure.Segments.Add(new LineSegment { Point = start });
            }
            else
            {
                // Start at start point
                figure.StartPoint = start;
                _hasSubPaths = true;
            }
            if (2 * Math.PI <= Math.Abs(endAngle - startAngle))
            {
                // Need to draw complete circles as two half-circles
                Arc(x, y, radius, startAngle, Math.PI, anticlockwise);
                startAngle = Math.PI;
            }
            // Normalize angles
            start = new Point(x + (radius * Math.Cos(startAngle)), y + (radius * Math.Sin(startAngle)));
            var end = new Point(x + (radius * Math.Cos(endAngle)), y + (radius * Math.Sin(endAngle)));
            if (anticlockwise)
            {
                if (startAngle < endAngle)
                {
                    startAngle += 2 * Math.PI;
                }
            }
            else
            {
                if (endAngle < startAngle)
                {
                    endAngle += 2 * Math.PI;
                }
            }
            // Add arc
            figure.Segments.Add(new ArcSegment { Point = end, Size = new Size(radius, radius), IsLargeArc = Math.PI < Math.Abs(endAngle - startAngle), SweepDirection = anticlockwise ? SweepDirection.Counterclockwise : SweepDirection.Clockwise });
        }
		
#if WEB
		[ScriptableMember (ScriptAlias = "arcTo")]
#endif
		public void ArcTo(double x1, double y1, double x2, double y2, double radius)
		{
			throw new NotImplementedException ();	
		}
	
			
#if WEB
		[ScriptableMember (ScriptAlias = "rect")]
#endif
        public void Rect(double x, double y, double w, double h)
        {
            MoveTo(x, y);
            LineTo(x + w, y);
            LineTo(x + w, y + h);
            LineTo(x, y + h);
            LineTo(x, y);
            MoveTo(x, y);
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "fill")]
#endif
        public void Fill()
        {
            _canvas.Children.Add(new Path { Data = Utility.ClonePathGeometry(_pathGeometry), Fill = _fill, Opacity = GlobalAlpha });
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "stroke")]
#endif
        public void Stroke()
        {
            _canvas.Children.Add(new Path { Data = Utility.ClonePathGeometry(_pathGeometry), Stroke = _stroke, StrokeThickness = LineWidth, Opacity = GlobalAlpha, StrokeStartLineCap = CapTypeToPenLineCap(_lineCap), StrokeEndLineCap = CapTypeToPenLineCap(_lineCap), StrokeLineJoin = JoinTypeToPenLineJoin(_lineJoin), StrokeMiterLimit = MiterLimit });
        }
		
#if WEB
		[ScriptableMember (ScriptAlias = "clip")]
#endif
		public void Clip()
		{
			throw new NotImplementedException ();
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "isPointInPath")]
#endif
		public bool IsPointInPath(double x, double y)
		{
			throw new NotImplementedException ();		
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "font")]
#endif
		public string Font {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "textAlign")]
#endif
		public string TextAlign {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "textBaseline")]
#endif
		public string TextBaseline {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "fillText")]
#endif
		public void FillText(string text, double x, double y, double maxWidth)
		{
			throw new NotImplementedException ();		
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "fillText")]
#endif
		public void FillText(string text, double x, double y)
		{
			throw new NotImplementedException ();		
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "strokeText")]
#endif
		public void StrokeText(string text, double x, double y, double maxWidth)
		{
			throw new NotImplementedException ();		
		}
			
#if WEB
		[ScriptableMember (ScriptAlias = "strokeText")]
#endif
		public void StrokeText(string text, double x, double y)
		{
			throw new NotImplementedException ();
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "drawImage")]
		public void DrawImage(ScriptObject image, double dx, double dy)
#else
        public void DrawImage(Cirrus.Gfx.Image image, double dx, double dy)
#endif
        {
            var imageElement = new Image { Source = Utility.DomImageSource(image), Opacity = GlobalAlpha };
            System.Windows.Controls.Canvas.SetLeft(imageElement, dx);
            System.Windows.Controls.Canvas.SetTop(imageElement, dy);
            _canvas.Children.Add(imageElement);
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "drawImage")]
		public void DrawImage(ScriptObject image, double dx, double dy, double dw, double dh)
#else
     	public void DrawImage(Cirrus.Gfx.Image image, double dx, double dy, double dw, double dh)
#endif
        {
            var imageElement = new Image { Source = Utility.DomImageSource(image), Width = dw, Height = dh, Stretch = Stretch.Fill, Opacity = GlobalAlpha };
            System.Windows.Controls.Canvas.SetLeft(imageElement, dx);
            System.Windows.Controls.Canvas.SetTop(imageElement, dy);
            _canvas.Children.Add(imageElement);
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "drawImage")]
		public void DrawImage(ScriptObject image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh)

#else
        public void DrawImage(Cirrus.Gfx.Image image, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh)
#endif
        {
            var imageElement = new Image { Stretch = Stretch.Fill, Opacity = GlobalAlpha };
            imageElement.Clip = new RectangleGeometry();
            _canvas.Children.Add(imageElement);
#if WPF
            imageElement.Loaded += delegate
#else
            imageElement.ImageOpened += delegate
#endif
            {
                // Calculate bounds to apply specified bounds
                var xm = dw / sw;
                var ym = dh / sh;
                System.Windows.Controls.Canvas.SetLeft(imageElement, dx - (sx * xm));
                System.Windows.Controls.Canvas.SetTop(imageElement, dy - (sy * ym));
                imageElement.Width = imageElement.ActualWidth * xm;
                imageElement.Height = imageElement.ActualHeight * ym;
                imageElement.Clip = new RectangleGeometry { Rect = new Rect(sx * xm, sy * ym, dw, dh) };
            };
            imageElement.Source = Utility.DomImageSource(image);
        }
	
        private enum CapTypes { butt, round, square };
        private static PenLineCap CapTypeToPenLineCap(CapTypes capType)
        {
            switch (capType)
            {
                case CapTypes.butt:
                    return PenLineCap.Flat;
                case CapTypes.round:
                    return PenLineCap.Round;
                case CapTypes.square:
                    return PenLineCap.Square;
                default:
                    throw new NotSupportedException();
            }
        }

        private enum JoinTypes { round, bevel, miter };
        private static PenLineJoin JoinTypeToPenLineJoin(JoinTypes joinType)
        {
            switch (joinType)
            {
                case JoinTypes.bevel:
                    return PenLineJoin.Bevel;
                case JoinTypes.miter:
                    return PenLineJoin.Miter;
                case JoinTypes.round:
                    return PenLineJoin.Round;
                default:
                    throw new NotSupportedException();
            }
        }

        // Represents a drawing state
        private class DrawingState
        {
            public object strokeStyle;
            public object fillStyle;
            public double globalAlpha;
            public double lineWidth;
            public string lineCap;
            public string lineJoin;
            public double miterLimit;
            public TransformGroup transformGroup;
        }
    }
}
