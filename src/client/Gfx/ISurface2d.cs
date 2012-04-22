using System;

using Cirrus.Gfx.Drawing;

namespace Cirrus.Gfx {

	/// <summary>
	/// A 2d graphical area.
	/// </summary>
	public interface ISurface2d {
		int Width  { get; }
		int Height { get; }

		IContext2d GetDrawingContext ();
	}
}

