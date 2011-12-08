using System;

namespace Cirrus.Events {
	
	// FIXME: Things like which button was used (or finger for touch devices), xy position, etc..
	public class MouseEvent {
		public double X { get; set; }
		public double Y { get; set; }
	}
	
	public class MouseMove : MouseEvent {
	}

	public class MouseDown : MouseEvent {
	}
	
	public class MouseUp : MouseEvent {
	}
	
	public class Click : MouseEvent {	
	}
}

