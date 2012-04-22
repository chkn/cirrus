using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cirrus.Gfx.Imaging.Formats {

	public unsafe class Image8bpp : Image<Pixel8bpp> {

		Pixel8bpp* data;

		public override int BytesPerPixel {
			get { return 1; }
		}

		public override IntPtr PixelData {
			get { return new IntPtr ((void*)data); }
			set { data = (Pixel8bpp*)value;  }
		}

		public override Pixel8bpp GetPixel (int x, int y)
		{
			return data [y*Width + x];
		}

		public override void SetPixel (int x, int y, Pixel8bpp px)
		{
			data [y*Width + x] = px;
		}
	}
}

