using System;

namespace Cirrus.Gfx {
	
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
	
	// FIXME: clean up
	public struct CanvasState<TFill,TStroke> {
		public TFill fill;
		public TStroke stroke;
		//cairo_filter_t patternQuality;
		public double globalAlpha;
		//public string textAlignment;
		//public string textBaseline;
		//rgba_t shadow;
		public double shadowBlur;
		public double shadowOffsetX;
		public double shadowOffsetY;
	}
	
	/*
	public struct CanvasStroke {
		public LineCap lineCap;
		public LineJoin lineJoin;
		public double lineWidth;
		public double miterLimit;
		
		public static readonly CanvasStroke Default = new CanvasStroke ()
		{
			lineCap = LineCap.Butt,
			lineJoin = LineJoin.Miter,
			lineWidth = 1,
			miterLimit = 10
		};
	}
	*/
}

