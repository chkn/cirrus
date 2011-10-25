using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#if WEB
using System.Windows.Browser;
#endif

namespace Cirrus.Silverlight {
	internal static class Utility {
		
        // Clones a PathGeometry because Silverlight doesn't support reuse
        public static PathGeometry ClonePathGeometry(PathGeometry pathGeometry)
        {
            var pathGeometryClone = new PathGeometry();
            foreach (var figure in pathGeometry.Figures)
            {
                var figureClone = new PathFigure { StartPoint = figure.StartPoint, IsClosed = figure.IsClosed, IsFilled = figure.IsFilled };
                foreach (var segment in figure.Segments)
                {
                    var lineSegment = segment as LineSegment;
                    var arcSegment = segment as ArcSegment;
                    var bezierSegment = segment as BezierSegment;
                    var quadraticBezierSegment = segment as QuadraticBezierSegment;
                    if (null != lineSegment)
                    {
                        figureClone.Segments.Add(new LineSegment { Point = lineSegment.Point });
                    }
                    else if (null != arcSegment)
                    {
                        figureClone.Segments.Add(new ArcSegment { Point = arcSegment.Point, RotationAngle = arcSegment.RotationAngle, Size = arcSegment.Size, SweepDirection = arcSegment.SweepDirection, IsLargeArc = arcSegment.IsLargeArc });
                    }
                    else if (null != quadraticBezierSegment)
                    {
                        figureClone.Segments.Add(new QuadraticBezierSegment { Point1 = quadraticBezierSegment.Point1, Point2 = quadraticBezierSegment.Point2 });
                    }
                    else if (null != bezierSegment)
                    {
                        figureClone.Segments.Add(new BezierSegment { Point1 = bezierSegment.Point1, Point2 = bezierSegment.Point2, Point3 = bezierSegment.Point3 });
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                pathGeometryClone.Figures.Add(figureClone);
            }
            return pathGeometryClone;
        }

        // Clones a TransformGroup for use with a new Canvas
        public static TransformGroup CloneTransformGroup(TransformGroup transformGroup)
        {
            var transformGroupClone = new TransformGroup();
            foreach (var transform in transformGroup.Children)
            {
                var translateTransform = transform as TranslateTransform;
                var rotateTransform = transform as RotateTransform;
                var scaleTransform = transform as ScaleTransform;
                if (null != translateTransform)
                {
                    transformGroupClone.Children.Add(new TranslateTransform { X = translateTransform.X, Y = translateTransform.Y });
                }
                else if (null != rotateTransform)
                {
                    transformGroupClone.Children.Add(new RotateTransform { Angle = rotateTransform.Angle });
                }
                else if (null != scaleTransform)
                {
                    transformGroupClone.Children.Add(new ScaleTransform { ScaleX = scaleTransform.ScaleX, ScaleY = scaleTransform.ScaleY });
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return transformGroupClone;
        }

        // Clones a GradientBrush so a TranslateTransform can be applied
        public static Brush CloneAndMapGradientBrush(Brush brush, double x, double y)
        {
            var gradientBrush = brush as GradientBrush;
            if (null == gradientBrush)
            {
                return brush;
            }
            GradientBrush gradientBrushClone = null;
            var linearGradientBrush = gradientBrush as LinearGradientBrush;
            var radialGradientBrush = gradientBrush as RadialGradientBrush;
            if (null != linearGradientBrush)
            {
                var linearGradientBrushClone = new LinearGradientBrush();
                linearGradientBrushClone.StartPoint = linearGradientBrush.StartPoint;
                linearGradientBrushClone.EndPoint = linearGradientBrush.EndPoint;
                gradientBrushClone = linearGradientBrushClone;
            }
            else if (null != radialGradientBrush)
            {
                var radialGradientBrushClone = new RadialGradientBrush();
                radialGradientBrushClone.GradientOrigin = radialGradientBrush.GradientOrigin;
                radialGradientBrushClone.Center = radialGradientBrush.Center;
                radialGradientBrushClone.RadiusX = radialGradientBrush.RadiusX;
                radialGradientBrushClone.RadiusY = radialGradientBrush.RadiusY;
                gradientBrushClone = radialGradientBrushClone;
            }
            else
            {
                throw new NotSupportedException();
            }
            foreach (var gradientStop in gradientBrush.GradientStops)
            {
                gradientBrushClone.GradientStops.Add(new GradientStop { Color = gradientStop.Color, Offset = gradientStop.Offset });
            }
            gradientBrushClone.MappingMode = gradientBrush.MappingMode;
            gradientBrushClone.Transform = new TranslateTransform { X = -x, Y = -y };
            return gradientBrushClone;
        }

		
#if WEB
		public static BitmapSource DomImageSource(ScriptObject img)
		{
			Uri src = new Uri(HtmlPage.Document.DocumentUri, (string)img.GetProperty("src"));
			
			if (src.Scheme == "data")
				return BitmapSourceForDataUri(src);
			else
				return new BitmapImage(src);
		}
#else
		public static BitmapSource DomImageSource(Cirrus.Gfx.Image img)
		{
			throw new NotImplementedException ();	
		}
#endif
		
		public static BitmapSource BitmapSourceForDataUri(Uri dataUri)
		{
			string dataString = dataUri.OriginalString;
			
			int b64Start = dataString.IndexOf(",");
			if (b64Start == -1) return null;
			string base64 = dataString.Substring(b64Start + 1);
			
			Stream stream = new MemoryStream(Convert.FromBase64String(base64));
			BitmapImage image = new BitmapImage();
#if WPF
			image.StreamSource = stream;		
#else
			image.SetSource(stream);
#endif
			
			return image;
		}
    }	
	
}

