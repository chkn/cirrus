/*
	Rect.cs: A C# port of CanvasLayers.Rectangle from canvaslayers.js
	  (https://bitbucket.org/ant512/canvaslayers)
  
	Copyright (c) 2011 Alexander Corrado
	Copyright (c) 2010 Antony Dzeryn

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
 */

using System;
using System.Collections.Generic;

using Cirrus.Events;

namespace Cirrus.Gfx {
	
	//FIXME: Convert from Javadoc style to .net doc style
	public class BoundingRect :
		ICloneable,
	// --events:
		IEventSource<BoundsChange>
	{
		
		private double x;
		public double X {
			get { return x; }
			set { 
				if (value != x) {
					var old = Clone ();
					x = value;
					FireBoundsChanged (old);
				}
			}
		}
		
		private double y;
		public double Y {
			get { return y; }
			set {
				if (value != y) {
					var old = Clone ();
					y = value;
					FireBoundsChanged (old);
				}
			}
		}
		
		private double width;
		public double Width {
			get { return width; }
			set {
				if (value != width) {
					var old = Clone ();
					width = value;
					FireBoundsChanged (old);
				}
			}
		}
		
		private double height;
		public double Height {
			get { return height; }
			set {
				if (value != height) {
					var old = Clone ();
					height = value;
					FireBoundsChanged (old);
				}
			}
		}
		
		public double X2 {
			get { return X + Width - 1; }
			set { Width = value - X + 1; }
		}
		
		public double Y2 {
			get { return Y + Height - 1; }
			set { Height = value - Y + 1; }
		}
		
		public BoundingRect Clone ()
		{
			return MemberwiseClone () as BoundingRect;
		}
		object ICloneable.Clone () { return Clone (); }
		
		/**
		 * Gets the intersect of this rectangle with the supplied argument.
		 * @param rect The rectangle to intersect with this.
		 * @return A rectangle that represents the intersection of the two rectangles.
		 */
		public BoundingRect GetIntersection (BoundingRect rect)
		{
			return new BoundingRect {
				X = this.X > rect.X ? this.X : rect.X,
				Y = this.Y > rect.Y ? this.Y : rect.Y,

				X2 = this.X2 < rect.X2 ? this.X2 : rect.X2,
				Y2 = this.Y2 < rect.Y2 ? this.Y2 : rect.Y2
			};
		}
		
		/**
		 * Gets the smallest rectangle capable of containing this rect and the supplied
		 * argument.
		 * @param rect The rectangle to add to this.
		 * @return The smallest rectangle that can contain this rect and the argument.
		 */
		public BoundingRect GetAddition (BoundingRect rect)
		{
			return new BoundingRect {
				X = this.X < rect.X ? this.X : rect.X,
				Y = this.Y < rect.Y ? this.Y : rect.Y,

				X2 = this.X2 > rect.X2 ? this.X2 : rect.X2,
				Y2 = this.Y2 > rect.Y2 ? this.Y2 : rect.Y2
			};
		}
		
		/**
		 * Clips this rectangle to the intersection with the supplied argument.
		 * @param rect The rectangle to clip to.
		 */
		public void ClipToIntersection (BoundingRect rect)
		{
			var intersect = GetIntersection (rect);
			
			this.X = intersect.X;
			this.Y = intersect.Y;
			this.Width = intersect.Width;
			this.Height = intersect.Height;
		}
		
		/**
		 * Increases the size of the rect to encompass the supplied argument.
		 * @param rect The rect to encompass.
		 */
		public void ExpandToInclude (BoundingRect rect)
		{
			var addition = GetAddition (rect);
			
			this.X = addition.X;
			this.Y = addition.Y;
			this.Width = addition.Width;
			this.Height = addition.Height;
		}
		
		public bool SameSize (BoundingRect rect)
		{
			return this.Width == rect.Width &&
			       this.Height == rect.Height;
		}
		
		public bool SamePosition (BoundingRect rect)
		{
			return this.X == rect.X &&
			       this.Y == rect.Y;
		}
		
		public BoundsChange UpdateBounds (double X, double Y, double Width, double Height)
		{
			var old = Clone ();
			
			x = X;
			y = Y;
			width = Width;
			height = Height;
			
			return FireBoundsChanged (old);
		}
		
		/**
		 * Check if this rectangle intersects the argument.
		 * @param rect The rect to check for an intersection.
		 * @return True if the rects intersect.
		 */
		public bool Intersects (BoundingRect rect) {
			return ((this.X + this.Width > rect.X) &&
			        (this.Y + this.Height > rect.Y) &&
			        (this.X < rect.X + rect.Width) &&
			        (this.Y < rect.Y + rect.Height));	
		}
		
		public bool Equals (BoundingRect rect) {
			return this.X == rect.X &&
			       this.Y == rect.Y &&
			       this.Width == rect.Width &&
			       this.Height == rect.Height;
		}
		
		public override bool Equals (object obj)
		{
			var rect = obj as BoundingRect;
			if (rect == null)
				return false;
			
			return Equals (rect);
		}

		public override int GetHashCode ()
		{
			unchecked {
				return x.GetHashCode () ^ y.GetHashCode () ^ width.GetHashCode () ^ height.GetHashCode ();
			}
		}
		
		public override string ToString ()
		{
			return string.Format ("[BoundingRect x: {0}, y: {1}, w: {2}, h: {3}]", X, Y, Width, Height);
		}
		
		public static bool operator == (BoundingRect r1, BoundingRect r2)
		{
			if (r1 == null && r2 == null)
				return true;
			else if (r1 == null && r2 != null)
				return false;
			
			return r1.Equals (r2);
		}
		
		public static bool operator != (BoundingRect r1, BoundingRect r2)
		{
			return !(r1 == r2);
		}
		
		/**
		 * Check if this rectangle contains the argument co-ordinate.
		 * @param x The x co-ordinate to check.
		 * @param y The y co-ordinate to check.
		 * @return True if this rect contains the argument co-ordinate.
		 */
		public bool Contains (double x, double y) {
			return ((x >= this.X) &&
			        (y >= this.Y) &&
			        (x < this.X + this.Width) &&
			        (y < this.Y + this.Height));
		}
		
		/**
		 * Splits the rect argument into the area that overlaps this rect (this is
		 * the return value) and an array of areas that do not overlap (this is the
		 * remainderRects argument, which must be passed as an empty array).
		 * @param rect The rectangle to intersect with this.
		 * @param remainderRects An empty array that will be populated with the areas
		 * of the rect parameter that do not intersect with this rect.
		 * @return The intersection of this rectangle and the rect argument.
		 */
		public BoundingRect SplitIntersection (BoundingRect rect, out List<BoundingRect> remainder)
		{
			remainder = new List<BoundingRect> ();
			if (!this.Intersects (rect)) {
				remainder.Add (rect);
				return null;
			}
			
			// Copy the properties of rect into intersection; we trim this to size later
			var intersection = rect;
			
			// Check for a non-overlapped rect on the left
			if (intersection.X < this.X) {
			        var left = new BoundingRect {
			        	X = intersection.X,
			        	Y = intersection.Y,
			        	Width = this.X - intersection.X,
			        	Height = intersection.Height
					};
			        
			        // Insert the rect
			        remainder.Add (left);
			        
			        // Adjust the dimensions of the intersection
			        intersection.X = this.X;
			        intersection.Width -= left.Width;
			}
			
			// Check for a non-overlapped rect on the right
			if (intersection.X + intersection.Width > this.X + this.Width) {
			        var right = new BoundingRect {
			        	X = this.X + this.Width,
			        	Y = intersection.Y,
			        	Width = intersection.Width - (this.X + this.Width - intersection.X),
			        	Height = intersection.Height
					};
				
			        // Insert the rect
			        remainder.Add (right);
			        
			        // Adjust dimensions of the intersection
			        intersection.Width -= right.Width;
			}
			
			// Check for a non-overlapped rect above
			if (intersection.Y < this.Y) {
			        var top = new BoundingRect {
			        	X = intersection.X,
			        	Y = intersection.Y,
			        	Width = intersection.Width,
			        	Height = this.Y - intersection.Y
					};
				
			        // Insert the rect
			        remainder.Add (top);
			        
			        // Adjust the dimensions of the intersection
			        intersection.Y = this.Y;
			        intersection.Height -= top.Height;
			}
			
			// Check for a non-overlapped rect below
			if (intersection.Y + intersection.Height > this.Y + this.Height) {
			        var bottom = new BoundingRect {
			        	X = intersection.X,
			        	Y = this.Y + this.Height,
			        	Width = intersection.Width,
			        	Height = intersection.Height - (this.Y + this.Height - intersection.Y)
					};
				
			        // Insert the rect
			        remainder.Add (bottom);
			        
			        // Adjust dimensions of the intersection
			        intersection.Height -= bottom.Height;
			}
			
			return intersection;
		}
		
		// events:
		public event Action<BoundsChange> BoundsChanged;
		private BoundsChange FireBoundsChanged (BoundingRect old)
		{
			var bc = new BoundsChange {
				Bounds = this,
				OldBounds = old
			};
			BoundsChanged.Fire (bc);
			return bc;
		}
		Future<BoundsChange> IEventSource<BoundsChange>.GetFuture () { return BoundsChanged.ToFuture (); }
	}
}

