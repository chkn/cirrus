using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Cirrus.Gfx.Drawing;
using Cirrus.Gfx.Imaging.Formats;

namespace Cirrus.Gfx.Imaging {

	/// <summary>
	/// An ISurface2d held in system memory.
	/// </summary>
	public abstract class Image : IDisposable, ISurface2d {
		public int Width  { get; protected set; }
		public int Height { get; protected set; }

		public abstract IntPtr PixelData  { get; set; }
		public abstract int BytesPerRow   { get; }
		public abstract int BytesPerPixel { get; }

		public virtual void Dispose () {}
		public virtual IContext2d GetDrawingContext ()
		{
			throw new NotImplementedException ();
		}

		static Image ()
		{
			// Format list here:
			Image<Pixel8bpp>.Factory = () => new Image8bpp ();
			Image<Pixel32bpp>.Factory = () => new Image32bpp ();
		}
	}

	public abstract class Image<TPixel> : Image
		where TPixel : struct
	{
		internal static Func<Image<TPixel>> Factory;

		protected Action<IntPtr> dealloc;
		protected static Action<IntPtr> dealloc_marshal_freehglobal = ptr => Marshal.FreeHGlobal (ptr);

		public override int BytesPerRow {
			get { return BytesPerPixel * Width; }
		}

		public override void Dispose ()
		{
			if (dealloc != null) {
				dealloc (PixelData);
				dealloc = null;
			}
		}

		public void Filter (Func<TPixel,TPixel> filter)
		{
			for (var y = 0; y < Height; y++) {
				for (var x = 0; x < Width; x++)
					SetPixel (x, y, filter (GetPixel (x, y)));
			}
		}

		public Image<TPixel> RotateAndFlip ()
		{
			// set x = y, y = x
			var result = Image<TPixel>.Create (Height, Width);
			for (var y = 0; y < Height; y++) {
				for (var x = 0; x < Width; x++)
					result.SetPixel (Height - y - 1, Width - x - 1, GetPixel (x, y));
			}
			return result;
		}

		public Image<TDestPixel> Convert<TDestPixel> (Func<TPixel,TDestPixel> converter)
			where TDestPixel : struct
		{
			var result = Image<TDestPixel>.Create (Width, Height);
			for (var y = 0; y < Height; y++) {
				for (var x = 0; x < Width; x++)
					result.SetPixel (x, y, converter (GetPixel (x, y)));
			}
			return result;
		}

		public void Draw (Image<TPixel> source, int sx, int sy)
		{
			Draw<TPixel> (source, sx, sy, px => px);
		}

		public void Draw<TSrcPixel> (Image<TSrcPixel> source, int sx, int sy, Func<TSrcPixel,TPixel> converter)
			where TSrcPixel : struct
		{
			var w = Math.Min (Width, source.Width);
			var h = Math.Min (Height, source.Height);
			for (var y = 0; y < h; y++) {
				for (var x = 0; x < w; x++)
					SetPixel (x, y, converter (source.GetPixel (x + sx, y + sy)));
			}
		}

		public static Image<TPixel> Create (int width, int height)
		{
			return Create (Marshal.AllocHGlobal (height*width*Marshal.SizeOf (typeof (TPixel))), width, height, dealloc_marshal_freehglobal);
		}

		public static Image<TPixel> Create (IntPtr data, int width, int height, Action<IntPtr> dealloc)
		{
			if (Factory == null)
				throw new NotSupportedException ("Unsupported pixel format: " + typeof (TPixel).Name);

			var image = Factory ();

			image.PixelData = data;
			image.Width = width;
			image.Height = height;
			image.dealloc = dealloc;

			return image;
		}

		public abstract TPixel GetPixel (int x, int y);
		public abstract void SetPixel (int x, int y, TPixel px);
	}


}

