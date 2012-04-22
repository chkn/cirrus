using System;

namespace Cirrus.Gfx.Imaging.Formats {

	public unsafe class Image32bpp : Image<Pixel32bpp> {

		Pixel32bpp* data;

		public override int BytesPerPixel {
			get { return 4; }
		}

		public override IntPtr PixelData {
			get { return new IntPtr ((void*)data); }
			set { data = (Pixel32bpp*)value;    }
		}

		public override Pixel32bpp GetPixel (int x, int y)
		{
			return data [y*Width + x];
		}

		public override void SetPixel (int x, int y, Pixel32bpp px)
		{
			data [y*Width + x] = px;
		}
	}
}

