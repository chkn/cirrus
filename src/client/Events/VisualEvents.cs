using System;

using Cirrus.Gfx;

namespace Cirrus.Events {
	
	public class VisualEvent {
		
		// The new/current bounds
		public BoundingRect Bounds { get; set; }
	}
	
	public class BoundsChange : VisualEvent {
		
		public BoundingRect OldBounds { get; set; }
		
		public bool PositionChanged {
			get { return OldBounds == null || !Bounds.SamePosition (OldBounds); }
		}
		
		public bool SizeChanged {
			get { return OldBounds == null || !Bounds.SameSize (OldBounds); }
		}
		
		public override string ToString () { return string.Format ("[BoundsChange new: {0}, old: {1}]", Bounds, OldBounds); }
	}
}

