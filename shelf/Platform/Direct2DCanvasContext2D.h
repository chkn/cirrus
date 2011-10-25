#include <stack>
#include <d2d1.h>
#using <mscorlib.dll>

using namespace System;
using namespace System::Collections::Generic;
using namespace Cirrus::Gfx;

ref class Direct2DCanvasContext2D : ICanvasRenderingContext2D {
private:
	ID2D1RenderTarget* ctx;
	CanvasState state;
	Stack<CanvasState>^ states;
	std::stack<ID2D1DrawingStateBlock*>* d2d_states;

public:

	property ICanvas^ Canvas { 
		virtual ICanvas^ get ();
	}
		
	Direct2DCanvasContext2D (ID2D1RenderTarget* ctx);
		
	virtual void Save ();
	virtual void Restore ();

	virtual void Scale (double x, double y);
	virtual void Rotate (double angle);
	virtual void Translate (double x, double y);
	virtual void Transform (double a, double b, double c, double d, double e, double f);

	virtual ICanvasGradient^ CreateLinearGradient (double x0, double y0, double x1, double y1);
	virtual ICanvasGradient^ CreateRadialGradient (double x0, double y0, double r0, double x1, double y1, double r1);

	virtual void ClearRect (double x, double y, double w, double h);
	virtual void FillRect (double x, double y, double w, double h);
	virtual void StrokeRect (double x, double y, double w, double h);

	virtual void BeginPath ();
	virtual void ClosePath ();
	virtual void MoveTo (double x, double y);
	virtual void LineTo (double x, double y);
	virtual void QuadraticCurveTo (double cpx, double cpy, double x, double y);
	virtual void BezierCurveTo (double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);
	virtual void ArcTo (double x1, double y1, double x2, double y2, double radius);
	virtual void Rect (double x, double y, double w, double h);
	virtual void Arc (double x, double y, double radius, double startAngle, double endAngle);
	virtual void Arc (double x, double y, double radius, double startAngle, double endAngle, bool anticlockwise);

	virtual void Fill ();
	virtual void Stroke ();
	virtual void Clip ();
	virtual bool IsPointInPath (double x, double y);

	virtual void FillText (String^ text, double x, double y);
	virtual void FillText (String^ text, double x, double y, double maxWidth);
	virtual void StrokeText (String^ text, double x, double y);
	virtual void StrokeText (String^ text, double x, double y, double maxWidth);

	property double GlobalAlpha {
		virtual double get ();
		virtual void set (double value);
	}

	property String^ GlobalCompositeOperation {
		virtual String^ get ();
		virtual void set (String^ value);
	}

	property Object^ StrokeStyle {
		virtual Object^ get ();
		virtual void set (Object^ value);
	}

	property Object^ FillStyle {
		virtual Object^ get ();
		virtual void set (Object^ value);
	}

	property double LineWidth {
		virtual double get ();
		virtual void set (double value);
	}

	property Cirrus::Gfx::LineCap LineCap {
		virtual Cirrus::Gfx::LineCap get ();
		virtual void set (Cirrus::Gfx::LineCap value);
	}

	property Cirrus::Gfx::LineJoin LineJoin {
		virtual Cirrus::Gfx::LineJoin get ();
		virtual void set (Cirrus::Gfx::LineJoin value);
	}

	property double MiterLimit {
		virtual double get ();
		virtual void set (double value);
	}

	property double ShadowOffsetX {
		virtual double get ();
		virtual void set (double value);
	}

	property double ShadowOffsetY {
		virtual double get ();
		virtual void set (double value);
	}

	property double ShadowBlur {
		virtual double get ();
		virtual void set (double value);
	}

	property String^ ShadowColor {
		virtual String^ get ();
		virtual void set (String^ value);
	}

	property String^ Font {
		virtual String^ get ();
		virtual void set (String^ value);
	}

	property String^ TextAlign {
		virtual String^ get ();
		virtual void set (String^ value);
	}

	property String^ TextBaseline {
		virtual String^ get ();
		virtual void set (String^ value);
	}

};