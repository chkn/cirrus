using System;
namespace Cirrus.Gfx {
	public interface ICanvasGradient {
		void AddColorStop(double offset, string cssColor);
		
	}
}

