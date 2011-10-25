#include <windows.h>
#include <d2d1.h>

#using <mscorlib.dll>

using namespace System;
using namespace Cirrus::Gfx;

ref class Direct2DCanvas : ICanvas {
private:
	ID2D1HwndRenderTarget* render_target;
	int width, height;

public:

	static property ID2D1Factory* Factory {
		ID2D1Factory* get ();
	}

	Direct2DCanvas (HWND hwnd, int width, int height);

	property int Width {
		virtual int get ();
		virtual void set (int w);
	}

	property int Height {
		virtual int get ();
		virtual void set (int h);
	}

	virtual ICanvasRenderingContext2D^ GetContext (String^ id);

};