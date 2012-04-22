using System;

namespace Cirrus.Gfx.Drawing {

	public enum LineCap {
	    Butt,
	    Round,
	    Square
	};
	
	public enum LineJoin {
	    Miter,
	    Round,
	    Bevel
	};

	// Based on HTML5 Canvas API
	// http://www.whatwg.org/specs/web-apps/current-work/multipage/the-canvas-element.html#canvasrenderingcontext2d
	public interface IContext2d {
		// state
		void Save ();    // push state on state stack
		void Restore (); // pop state stack and restore state

		// transformations (default transform is the identity matrix)
		void Scale (double x, double y);
		void Rotate (double angle);
		void Translate (double x, double y);
		void Transform (double a, double b, double c, double d, double e, double f);

		// compositing
		double GlobalAlpha { get; set; }              // (default 1.0)
		string GlobalCompositeOperation { get; set; } // (default source-over)

  		// colors and styles
        object StrokeStyle { get; set; } // (default black)
        object FillStyle   { get; set; } // (default black)
  		//ICanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1);
  		//ICanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1);
  		//ICanvasPattern CreatePattern(in HTMLImageElement image, in DOMString repetition);
  		//ICanvasPattern CreatePattern(in HTMLCanvasElement image, in DOMString repetition);
  		//ICanvasPattern CreatePattern(in HTMLVideoElement image, in DOMString repetition);

		// line caps/joins
		double LineWidth  { get; set; } // (default 1)
		LineCap LineCap   { get; set; } // "butt", "round", "square" (default "butt")
		LineJoin LineJoin { get; set; } // "round", "bevel", "miter" (default "miter")
		double MiterLimit { get; set; } // (default 10)
		
		// shadows
		double ShadowOffsetX { get; set; } // (default 0)
		double ShadowOffsetY { get; set; } // (default 0)
		double ShadowBlur { get; set; } // (default 0)
		string ShadowColor { get; set; } // (default transparent black)
		
		// rects
		void ClearRect (double x, double y, double w, double h);
		void FillRect (double x, double y, double w, double h);
		void StrokeRect (double x, double y, double w, double h);
		
		// path API
		void BeginPath ();
		void ClosePath ();
		void MoveTo (double x, double y);
		void LineTo (double x, double y);
		void QuadraticCurveTo (double cpx, double cpy, double x, double y);
		void BezierCurveTo (double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);
		void ArcTo (double x1, double y1, double x2, double y2, double radius);
		void Rect (double x, double y, double w, double h);
		void Arc (double x, double y, double radius, double startAngle, double endAngle, bool anticlockwise);
		void Arc (double x, double y, double radius, double startAngle, double endAngle);
		void Fill ();
		void Stroke ();
		void Clip ();
		bool IsPointInPath (double x, double y);

		// focus management
		//bool DrawFocusRing(in Element element, in double xCaret, in double yCaret, in optional boolean canDrawCustom);

		// text
		string Font         { get; set; } // (default 10px sans-serif)
		string TextAlign    { get; set; } // "start", "end", "left", "right", "center" (default: "start")
		string TextBaseline { get; set; } // "top", "hanging", "middle", "alphabetic", "ideographic", "bottom" (default: "alphabetic")
		void FillText (string text, double x, double y, double maxWidth);
		void FillText (string text, double x, double y);
		void StrokeText (string text, double x, double y, double maxWidth);
		void StrokeText (string text, double x, double y);
		//TextMetrics measureText(in DOMString text);
		
		/*
		// drawing images
		void drawImage(in HTMLImageElement image, in double dx, in double dy, in optional double dw, in double dh);
		void drawImage(in HTMLImageElement image, in double sx, in double sy, in double sw, in double sh, in double dx, in double dy, in double dw, in double dh);
		void drawImage(in HTMLCanvasElement image, in double dx, in double dy, in optional double dw, in double dh);
		void drawImage(in HTMLCanvasElement image, in double sx, in double sy, in double sw, in double sh, in double dx, in double dy, in double dw, in double dh);
		void drawImage(in HTMLVideoElement image, in double dx, in double dy, in optional double dw, in double dh);
		void drawImage(in HTMLVideoElement image, in double sx, in double sy, in double sw, in double sh, in double dx, in double dy, in double dw, in double dh);
		 */
		 
		/*
		// pixel manipulation
		ImageData createImageData(in double sw, in double sh);
		ImageData createImageData(in ImageData imagedata);
		ImageData getImageData(in double sx, in double sy, in double sw, in double sh);
		void putImageData(in ImageData imagedata, in double dx, in double dy, in optional double dirtyX, in double dirtyY, in double dirtyWidth, in double dirtyHeight);
		 */
	}
}

