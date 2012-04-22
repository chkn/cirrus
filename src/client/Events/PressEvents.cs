using System;

namespace Cirrus.Events {
	
	// FIXME: Things like which button was used (or finger for touch devices), xy position, etc..
	public class PressEvent {
		public double X { get; set; }
		public double Y { get; set; }
	}
	
	public class PressMove : PressEvent {
	}

	public class PressDown : PressEvent {
	}
	
	public class PressUp : PressEvent {
	}
	
	public class Click : PressEvent {
	}
}

