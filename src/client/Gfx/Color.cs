using System;

namespace Cirrus.Gfx {
	
	public struct Color {
		
		public uint argb32;
		
		public byte A {
			get { return (byte)((argb32 >> 24) & 0xff); }
            set { argb32 |= ((uint)value << 24); }
		}
		
		public byte R {
			get { return (byte)((argb32 >> 16) & 0xff); }
            set { argb32 |= ((uint)value << 16); }
		}
		
		public byte G {
			get { return (byte)((argb32 >> 8) & 0xff); }
            set { argb32 |= ((uint)value << 8); }
		}
		
		public byte B {
			get { return (byte)(argb32 & 0xff); }
			set { argb32 |= (uint)value; }
		}
		
		public Color (uint argb32)
		{
			this.argb32 = argb32;
		}
		
		public Color (byte r, byte g, byte b)
		{
			this.argb32 = ((uint)255 << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
		}
		
		// Some common colors:
		public static readonly Color Black = new Color (0xff000000);
		public static readonly Color White = new Color (0xffffffff);
		public static readonly Color Red   = new Color (0xffff0000);
		public static readonly Color Green = new Color (0xff00ff00);
		public static readonly Color Blue  = new Color (0xff0000ff);
		
	}
}

