using System;

namespace Cirrus.Gfx {
	
	public interface ICanvas {
		ICanvasContext2D GetContext2D ();
		int Width { get; set; }
		int Height { get; set; }
	}
}

