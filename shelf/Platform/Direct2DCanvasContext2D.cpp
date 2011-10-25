#include "Stdafx.h"
#include "Direct2DCanvas.h"
#include "Direct2DCanvasContext2D.h"

// utility forward decl:
D2D1::Matrix3x2F* GetTransform (ID2D1RenderTarget* ctx);

Direct2DCanvasContext2D::Direct2DCanvasContext2D (ID2D1RenderTarget* ctx)
{
	this->ctx = ctx;
	this->state = CanvasState::Default;
	this->states = gcnew Stack<CanvasState> ();
	this->d2d_states = new std::stack<ID2D1DrawingStateBlock*>();
}

void Direct2DCanvasContext2D::Save ()
{
	ID2D1DrawingStateBlock* sblock;
	Direct2DCanvas::Factory->CreateDrawingStateBlock (&sblock);

	ctx->SaveDrawingState (sblock);

	this->d2d_states->push (sblock);
	this->states->Push (this->state);
}

void Direct2DCanvasContext2D::Restore ()
{
	ID2D1DrawingStateBlock* sblock = this->d2d_states->top ();

	ctx->RestoreDrawingState (sblock);

	this->d2d_states->pop ();
	this->state = this->states->Pop ();
}

void Direct2DCanvasContext2D::Scale (double x, double y)
{
	D2D1::Matrix3x2F* tx = GetTransform (this->ctx);
	ctx->SetTransform (D2D1::Matrix3x2F::Scale (x, y) * (*tx));
	delete tx;
}

D2D1::Matrix3x2F* GetTransform (ID2D1RenderTarget* ctx)
{
	D2D1::Matrix3x2F result;
	D2D1_MATRIX_3X2_F mtx;
	ctx->GetTransform (&mtx);
	return result.ReinterpretBaseType(&mtx);
}