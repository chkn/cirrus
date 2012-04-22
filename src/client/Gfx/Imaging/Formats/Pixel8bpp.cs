using System;
using System.Runtime.InteropServices;

namespace Cirrus.Gfx.Imaging.Formats {

	[StructLayout (LayoutKind.Sequential, Size=1, Pack=1)]
	public struct Pixel8bpp {

		public byte B;

		public static implicit operator Pixel8bpp (byte px)
		{
			return new Pixel8bpp { B = px };
		}

		public static implicit operator byte (Pixel8bpp px)
		{
			return px.B;
		}

		public static implicit operator int (Pixel8bpp px)
		{
			return (int)px.B;
		}
	}
}

