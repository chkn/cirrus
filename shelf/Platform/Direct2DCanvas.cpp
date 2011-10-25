
#include "Stdafx.h"
#include "Direct2DCanvas.h"
#include "Direct2DCanvasContext2D.h"

static ID2D1Factory* d2d = NULL;

ID2D1Factory* Direct2DCanvas::Factory::get ()
{
	// create D2D factory if we don't have a static one, and create a new Hwnd render target
	if (!d2d) {
		if (!SUCCEEDED(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &d2d)))
			throw gcnew Exception ("Could not init Direct2D");
	}
	return d2d;
}

Direct2DCanvas::Direct2DCanvas (HWND hwnd, int width, int height)
:	width(width),
	height(height)
{
	D2D1_SIZE_U size = D2D1::SizeU(static_cast<UINT32> (width), static_cast<UINT32> (height));

	// create new D2D render target for our hwnd
	pin_ptr<ID2D1HwndRenderTarget*> rtptr = &this->render_target;
	if (!SUCCEEDED(Factory->CreateHwndRenderTarget(
		&D2D1::RenderTargetProperties(),
		&D2D1::HwndRenderTargetProperties(hwnd, size),
		rtptr
	)))
	{
		throw gcnew Exception ("Could not create render target for HWND.");
	}
}

int Direct2DCanvas::Width::get ()
{
	return this->width;
}

void Direct2DCanvas::Width::set (int w)
{
	throw gcnew NotImplementedException ();
}

int Direct2DCanvas::Height::get ()
{
	return this->height;
}

void Direct2DCanvas::Height::set (int h)
{
	throw gcnew NotImplementedException ();
}

ICanvasRenderingContext2D^ Direct2DCanvas::GetContext (String^ id)
{
	if (id->Equals ("2d"))
		return gcnew Direct2DCanvasContext2D (render_target);

	return nullptr;
}