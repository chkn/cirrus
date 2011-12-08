using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Cirrus;
using Cirrus.Events;

namespace Cirrus.Gfx {	
	
	/// <summary>
	/// A method that represents the rendering logic for a Layer2D.
	/// </summary>
	/// <remarks>
	/// See the discussion in <see cref="M:Layer2D.Render(Cirrus.Gfx.BoundingRect,Cirrus.Gfx.ICanvasContext2D)"/>
	///  for implementation guidelines for a Layer2D rendering method.
	/// </remarks>
	public delegate Future Layer2DRenderer (Layer2D layer, BoundingRect rect, ICanvasContext2D ctx);
	
	/// <summary>
	/// A Layer2D is a two-dimensional graphical object that can be positioned and
	/// resized within a Canvas.
	/// </summary>
	public class Layer2D :
		
	// --visual events:
		IEventSource<BoundsChange>
	{

		protected ICanvas Canvas { get; set; }
		private bool transform;
		private Future renderFiber;

		// either set Renderer OR override Render method in a subclass
		public Layer2DRenderer Renderer { get; set; }
		public BoundingRect Bounds { get; private set; }
		public Layer2D Parent { get; private set; }
		
		// Scaling can be accomplished by either setting the ScaleX/ScaleY properties
		//  OR the ContentWidth/ContentHeight properties.. whichever is easier. Setting one overrides the other.
		
		private double? content_width;
		public  double ContentWidth {
			get {
				return content_width ?? (Bounds.Width / scale_x.Value);
			}
			set {
				scale_x = null;
				content_width = value;
			}
		}
		
		private double? scale_x;
		public  double ScaleX {
			get {
				return scale_x ?? (Bounds.Width / content_width.Value);
			}
			set {
				content_width = null;
				scale_x = value;
			}
		}
		
		private double? content_height;
		public  double ContentHeight {
			get {
				return content_height ?? (Bounds.Height / scale_y.Value);
			}
			set {
				scale_y = null;
				content_height = value;
			}
		}
		
		private double? scale_y;
		public  double ScaleY {
			get {
				return scale_y ?? (Bounds.Height / content_height.Value);
			}
			set {
				content_height = null;
				scale_y = value;
			}
		}
		
		public Layer2D (Layer2D parent)
			: this (parent.Canvas, true)
		{
			this.Parent = parent;
			SetupEventHandlers ();
		}
		
		// Subclasses must call SetupEventHandlers!
		protected Layer2D (ICanvas canvas, bool transform)
		{
			this.Canvas = canvas;
			this.ScaleX = 1;
			this.ScaleY = 1;
			
			this.transform = transform;
			
			this.Bounds = new BoundingRect {
				X = 0,
				Y = 0,
				Width = canvas.Width,
				Height = canvas.Height
			};
		}
		
		public void ApplyTransform (ICanvasContext2D ctx)
		{
			if (Parent != null)
				Parent.ApplyTransform (ctx);
			
			if (transform) {
				// transform the context relative to this layer
				
            	ctx.Translate (Bounds.X, Bounds.Y);
				
				// clip the context to this layers's dimensions
            	ctx.BeginPath ();
            	ctx.Rect (0, 0, Bounds.Width, Bounds.Height);
            	ctx.Clip ();
            
				// if a scale factor is set, do that too
            	ctx.Scale (ScaleX, ScaleY);
			}
		}
		
		public void Render ()
		{
			Render (Bounds);
		}
		
		// FIXME: Only render dirty rects
		public void Render (BoundingRect rect)
		{
			var ctx = Canvas.GetContext2D ();
			ApplyTransform (ctx);			
			
			// If we were alredy rendering, stop it
			if (renderFiber != null && renderFiber.Status == FutureStatus.Pending)
				renderFiber.Cancel ();
			
			renderFiber = Render (rect, ctx);
			
			// Render children last
			//foreach (var child in Children) {
			//	child.Render (rect.GetIntersection (child.Bounds));
			//}
		}
		
		/// <summary>
		/// Contains the rendering logic for this Layer2D.
		/// </summary>
		/// <remarks>
		/// The rendering logic in a Layer2D can be defined either by setting the <see cref="P:Renderer"/> property,
		///  or by overriding this method in a subclass.
		/// In either case, the rendering routine may be a fiber that continues rendering, possibly in an infinite loop (i.e for animation),
		///  or it can be a one-off draw something and return routine. If the first case is desired, it is recommended that
		///  it be implemented as an asynchronous method that is processed by the postcompiler. At the least, a Future that supports cancellation
		///  must be returned if the method does not complete synchronously.
		/// For an explanation, take an animation that continues in an indefinite loop, delaying for a certain period between iterations.
		///  If another layer is placed atop the animation layer, and then moved, exposing a previously unexposed region of the animation layer,
		///  that region needs to be drawn expeditiously. To accomplish that, the view system will cancel the animation's Future (as returned by
		///  a previous invocation of this method), and immediately invoke this method again. Thus, any state needed to correctly render the layer
		///  must not be stored in locals of an asynchronous method.
		/// </remarks>
		/// <param name='rect'>
		/// The BoundingRect into which rendering is required.
		/// </param>
		/// <param name='ctx'>
		/// The ICanvasContext2D into which rendering is required.
		/// </param>
		protected virtual Future Render (BoundingRect rect, ICanvasContext2D ctx)
		{
			if (Renderer != null)
				return Renderer (this, rect, ctx);
			
			return Future.Fulfilled;
		}
		
		
		// event consumers:
		
		protected virtual void SetupEventHandlers ()
		{
			this.OnEvery<BoundsChange> (HandleBoundsChange);
		}
		
		protected virtual void HandleBoundsChange (BoundsChange bc)
		{
			// if we moved, cause our parent to draw exposed region
			if (!bc.Bounds.SamePosition (bc.OldBounds) && Parent != null) {
				List<BoundingRect> dirtyRects;
				
				bc.OldBounds.SplitIntersection (bc.Bounds, out dirtyRects);
				dirtyRects.ForEach (Parent.Render);
			}
			
			// we need to rerender the whole thing on bounds change...
			// even if we didn't move, scaling could have affected our whole content.
			Render ();
		}
		
		// event sources:
		public virtual Future<BoundsChange> OnBoundsChange () { return Bounds.On<BoundsChange> (); }
		Future<BoundsChange> IEventSource<BoundsChange>.GetFuture () { return OnBoundsChange (); }

	}
}

