using System;
using System.Runtime.InteropServices;

namespace Cirrus.Gfx.Imaging.Formats {

	[StructLayout (LayoutKind.Sequential, Size=4, Pack=1)]
	public struct Pixel32bpp {
		
		public uint Argb32;
		
		public byte A {
			get { return (byte)((Argb32 >> 24) & 0xff); }
            set { Argb32 |= ((uint)value << 24); }
		}
		
		public byte R {
			get { return (byte)((Argb32 >> 16) & 0xff); }
            set { Argb32 |= ((uint)value << 16); }
		}
		
		public byte G {
			get { return (byte)((Argb32 >> 8) & 0xff); }
            set { Argb32 |= ((uint)value << 8); }
		}
		
		public byte B {
			get { return (byte)(Argb32 & 0xff); }
			set { Argb32 |= (uint)value; }
		}
		
		public Pixel32bpp (uint argb32)
		{
			this.Argb32 = argb32;
		}
		
		public Pixel32bpp (byte r, byte g, byte b)
		{
			this.Argb32 = ((uint)255 << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
		}
		
		// Some common colors:
		// From http://www.w3.org/TR/css3-color/

		public static readonly Pixel32bpp AliceBlue = new Pixel32bpp (0xfff0f8ff);
		public static readonly Pixel32bpp AntiqueWhite = new Pixel32bpp (0xfffaebd7);
		public static readonly Pixel32bpp Aqua = new Pixel32bpp (0xff00ffff);
		public static readonly Pixel32bpp AquaMarine = new Pixel32bpp (0xff7fffd4);
		public static readonly Pixel32bpp Azure = new Pixel32bpp (0xfff0ffff);
        public static readonly Pixel32bpp Beige = new Pixel32bpp (0xfff5f5dc);
        public static readonly Pixel32bpp Bisque = new Pixel32bpp (0xffffe4c4);
        public static readonly Pixel32bpp Black = new Pixel32bpp (0xff000000);
		public static readonly Pixel32bpp BlanchedAlmond = new Pixel32bpp (0xffffebcd);
        public static readonly Pixel32bpp Blue = new Pixel32bpp (0xff0000ff);
        public static readonly Pixel32bpp BlueViolet = new Pixel32bpp (0xff8a2be2);
        public static readonly Pixel32bpp Brown = new Pixel32bpp (0xffa52a2a);
        public static readonly Pixel32bpp BurlyWood = new Pixel32bpp (0xffdeb887);
        public static readonly Pixel32bpp CadetBlue = new Pixel32bpp (0xff5f9ea0);
        public static readonly Pixel32bpp Chartreuse = new Pixel32bpp (0xff7fff00);
        public static readonly Pixel32bpp Chocolate = new Pixel32bpp (0xffd2691e);
        public static readonly Pixel32bpp Coral = new Pixel32bpp (0xffff7f50);
        public static readonly Pixel32bpp CornflowerBlue = new Pixel32bpp (0xff6495ed);
		public static readonly Pixel32bpp Cornsilk = new Pixel32bpp (0xfffff8dc);
		public static readonly Pixel32bpp Crimson = new Pixel32bpp (0xffdc143c);
		public static readonly Pixel32bpp Cyan = new Pixel32bpp (0xff00ffff);
		public static readonly Pixel32bpp DarkBlue = new Pixel32bpp (0xff00008b);
        public static readonly Pixel32bpp DarkCyan = new Pixel32bpp (0xff008b8b);
        public static readonly Pixel32bpp DarkGoldenrod = new Pixel32bpp (0xffb8860b);
        public static readonly Pixel32bpp DarkGray = new Pixel32bpp (0xffa9a9a9);
		public static readonly Pixel32bpp DarkGrey = new Pixel32bpp (0xffa9a9a9);
		public static readonly Pixel32bpp DarkGreen = new Pixel32bpp (0xff006400);
		public static readonly Pixel32bpp DarkKhaki = new Pixel32bpp (0xffbdb76b);
		public static readonly Pixel32bpp DarkMagenta = new Pixel32bpp (0xff8b008b);
		public static readonly Pixel32bpp DarkOliveGreen = new Pixel32bpp (0xff556b2f);
        public static readonly Pixel32bpp DarkOrange = new Pixel32bpp (0xffff8c00);
		public static readonly Pixel32bpp DarkOrchid = new Pixel32bpp (0xff9932cc);
		public static readonly Pixel32bpp DarkRed = new Pixel32bpp (0xff8b0000);
		public static readonly Pixel32bpp DarkSalmon = new Pixel32bpp (0xffe9967a);
		public static readonly Pixel32bpp DarkSeaGreen = new Pixel32bpp (0xff8fbc8f);
		public static readonly Pixel32bpp DarkSlateBlue = new Pixel32bpp (0xff483d8b);
		public static readonly Pixel32bpp DarkSlateGray = new Pixel32bpp (0xff2f4f4f);
		public static readonly Pixel32bpp DarkSlateGrey = new Pixel32bpp (0xff2f4f4f);
		public static readonly Pixel32bpp DarkTurquoise = new Pixel32bpp (0xff00ced1);
		public static readonly Pixel32bpp DarkViolet = new Pixel32bpp (0xff9400d3);
		public static readonly Pixel32bpp DeepPink = new Pixel32bpp (0xffff1493);
		public static readonly Pixel32bpp DeepSkyBlue = new Pixel32bpp (0xff00bfff);
		public static readonly Pixel32bpp DimGray = new Pixel32bpp (0xff696969);
		public static readonly Pixel32bpp DimGrey = new Pixel32bpp (0xff696969);
		public static readonly Pixel32bpp DodgerBlue = new Pixel32bpp (0xff1e90ff);
		public static readonly Pixel32bpp Firebrick = new Pixel32bpp (0xffb22222);
		public static readonly Pixel32bpp FloralWhite = new Pixel32bpp (0xfffffaf0);
		public static readonly Pixel32bpp ForestGreen = new Pixel32bpp (0xff228b22);
        public static readonly Pixel32bpp Fuchsia = new Pixel32bpp (0xffff00ff);
        public static readonly Pixel32bpp Gainsboro = new Pixel32bpp (0xffdcdcdc);
        public static readonly Pixel32bpp GhostWhite = new Pixel32bpp (0xfff8f8ff);
        public static readonly Pixel32bpp Gold = new Pixel32bpp (0xffffd700);
        public static readonly Pixel32bpp Goldenrod = new Pixel32bpp (0xffdaa520);
		public static readonly Pixel32bpp Gray = new Pixel32bpp (0xff808080);
		public static readonly Pixel32bpp Grey = new Pixel32bpp (0xff808080);
		public static readonly Pixel32bpp Green = new Pixel32bpp (0xff008000);
        public static readonly Pixel32bpp GreenYellow = new Pixel32bpp (0xffadff2f);
		public static readonly Pixel32bpp Honeydew = new Pixel32bpp (0xfff0fff0);
		public static readonly Pixel32bpp HotPink = new Pixel32bpp (0xffff69b4);
		public static readonly Pixel32bpp IndianRed = new Pixel32bpp (0xffcd5c5c);
		public static readonly Pixel32bpp Indigo = new Pixel32bpp (0xff4b0082);
		public static readonly Pixel32bpp Ivory = new Pixel32bpp (0xfffffff0);
		public static readonly Pixel32bpp Khaki = new Pixel32bpp (0xfff0e68c);
		public static readonly Pixel32bpp Lavender = new Pixel32bpp (0xffe6e6fa);
		public static readonly Pixel32bpp LavenderBlush = new Pixel32bpp (0xfffff0f5);
		public static readonly Pixel32bpp LawnGreen = new Pixel32bpp (0xff7cfc00);
		public static readonly Pixel32bpp LemonChiffon = new Pixel32bpp (0xfffffacd);
		public static readonly Pixel32bpp LightBlue = new Pixel32bpp (0xffadd8e6);
		public static readonly Pixel32bpp LightCoral = new Pixel32bpp (0xfff08080);
		public static readonly Pixel32bpp LightCyan = new Pixel32bpp (0xffe0ffff);
		public static readonly Pixel32bpp LightGoldenrodYellow = new Pixel32bpp (0xfffafad2);
		public static readonly Pixel32bpp LightGray = new Pixel32bpp (0xffd3d3d3);
		public static readonly Pixel32bpp LightGrey = new Pixel32bpp (0xffd3d3d3);
		public static readonly Pixel32bpp LightGreen = new Pixel32bpp (0xff90ee90);
		public static readonly Pixel32bpp LightPink = new Pixel32bpp (0xffffb6c1);
		public static readonly Pixel32bpp LightSalmon = new Pixel32bpp (0xffffa07a);
		public static readonly Pixel32bpp LightSeaGreen = new Pixel32bpp (0xff20b2aa);
		public static readonly Pixel32bpp LightSkyBlue = new Pixel32bpp (0xff87cefa);
		public static readonly Pixel32bpp LightSlateGray = new Pixel32bpp (0xff778899);
		public static readonly Pixel32bpp LightSlateGrey = new Pixel32bpp (0xff778899);
		public static readonly Pixel32bpp LightSteelBlue = new Pixel32bpp (0xffb0c4de);
		public static readonly Pixel32bpp LightYellow = new Pixel32bpp (0xffffffe0);
		public static readonly Pixel32bpp Lime = new Pixel32bpp (0xff00ff00);
		public static readonly Pixel32bpp LimeGreen = new Pixel32bpp (0xff32cd32);
		public static readonly Pixel32bpp Linen = new Pixel32bpp (0xfffaf0e6);
		public static readonly Pixel32bpp Magenta = new Pixel32bpp (0xffff00ff);
		public static readonly Pixel32bpp Maroon = new Pixel32bpp (0xff800000);
		public static readonly Pixel32bpp MediumAquaMarine = new Pixel32bpp (0xff66cdaa);
		public static readonly Pixel32bpp MediumBlue = new Pixel32bpp (0xff0000cd);
		public static readonly Pixel32bpp MediumOrchid = new Pixel32bpp (0xffba55d3);
		/*
            {"mediumpurple", "#9370db"},
            {"mediumseagreen", "#3cb371"},
            {"mediumslateblue", "#7b68ee"},
            {"mediumspringgreen", "#00fa9a"},
            {"mediumturquoise", "#48d1cc"},
            {"mediumvioletred", "#c71585"},
            {"midnightblue", "#191970"},
            {"mintcream", "#f5fffa"},
            {"mistyrose", "#ffe4e1"},
            {"moccasin", "#ffe4b5"},
            {"navajowhite", "#ffdead"},
            {"navy", "#000080"},
            {"oldlace", "#fdf5e6"},
            {"olive", "#808000"},
            {"olivedrab", "#6b8e23"},
            {"orange", "#ffa500"},
            {"orangered", "#ff4500"},
            {"orchid", "#da70d6"},
            {"palegoldenrod", "#eee8aa"},
            {"palegreen", "#98fb98"},
            {"paleturquoise", "#afeeee"},
            {"palevioletred", "#db7093"},
            {"papayawhip", "#ffefd5"},
            {"peachpuff", "#ffdab9"},
            {"peru", "#cd853f"},
            {"pink", "#ffc0cb"},
            {"plum", "#dda0dd"},
            {"powderblue", "#b0e0e6"},
            {"purple", "#800080"},
            {"red", "#ff0000"},
            {"rosybrown", "#bc8f8f"},
            {"royalblue", "#4169e1"},
            {"saddlebrown", "#8b4513"},
            {"salmon", "#fa8072"},
            {"sandybrown", "#f4a460"},
            {"seagreen", "#2e8b57"},
            {"seashell", "#fff5ee"},
            {"sienna", "#a0522d"},
            {"silver", "#c0c0c0"},
            {"skyblue", "#87ceeb"},
            {"slateblue", "#6a5acd"},
            {"slategray", "#708090"},
            {"slategrey", "#708090"},
            {"snow", "#fffafa"},
            {"springgreen", "#00ff7f"},
            {"steelblue", "#4682b4"},
            {"tan", "#d2b48c"},
            {"teal", "#008080"},
            {"thistle", "#d8bfd8"},
            {"tomato", "#ff6347"},
            {"turquoise", "#40e0d0"},
            {"violet", "#ee82ee"},
            {"wheat", "#f5deb3"},
            {"white", "#ffffff"},
            {"whitesmoke", "#f5f5f5"},
            {"yellow", "#ffff00"},
            {"yellowgreen", "#9acd32"}
          */
	}
}

