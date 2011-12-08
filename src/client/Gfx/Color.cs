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
		// From http://www.w3.org/TR/css3-color/

		public static readonly Color AliceBlue = new Color (0xfff0f8ff);
		public static readonly Color AntiqueWhite = new Color (0xfffaebd7);
		public static readonly Color Aqua = new Color (0xff00ffff);
		public static readonly Color AquaMarine = new Color (0xff7fffd4);
		public static readonly Color Azure = new Color (0xfff0ffff);
        public static readonly Color Beige = new Color (0xfff5f5dc);
        public static readonly Color Bisque = new Color (0xffffe4c4);
        public static readonly Color Black = new Color (0xff000000);
		public static readonly Color BlanchedAlmond = new Color (0xffffebcd);
        public static readonly Color Blue = new Color (0xff0000ff);
        public static readonly Color BlueViolet = new Color (0xff8a2be2);
        public static readonly Color Brown = new Color (0xffa52a2a);
        public static readonly Color BurlyWood = new Color (0xffdeb887);
        public static readonly Color CadetBlue = new Color (0xff5f9ea0);
        public static readonly Color Chartreuse = new Color (0xff7fff00);
        public static readonly Color Chocolate = new Color (0xffd2691e);
        public static readonly Color Coral = new Color (0xffff7f50);
        public static readonly Color CornflowerBlue = new Color (0xff6495ed);
		public static readonly Color Cornsilk = new Color (0xfffff8dc);
		public static readonly Color Crimson = new Color (0xffdc143c);
		public static readonly Color Cyan = new Color (0xff00ffff);
		public static readonly Color DarkBlue = new Color (0xff00008b);
        public static readonly Color DarkCyan = new Color (0xff008b8b);
        public static readonly Color DarkGoldenrod = new Color (0xffb8860b);
        public static readonly Color DarkGray = new Color (0xffa9a9a9);
		public static readonly Color DarkGrey = new Color (0xffa9a9a9);
		public static readonly Color DarkGreen = new Color (0xff006400);
		public static readonly Color DarkKhaki = new Color (0xffbdb76b);
		public static readonly Color DarkMagenta = new Color (0xff8b008b);
		public static readonly Color DarkOliveGreen = new Color (0xff556b2f);
        public static readonly Color DarkOrange = new Color (0xffff8c00);
		public static readonly Color DarkOrchid = new Color (0xff9932cc);
		public static readonly Color DarkRed = new Color (0xff8b0000);
		public static readonly Color DarkSalmon = new Color (0xffe9967a);
		public static readonly Color DarkSeaGreen = new Color (0xff8fbc8f);
		public static readonly Color DarkSlateBlue = new Color (0xff483d8b);
		public static readonly Color DarkSlateGray = new Color (0xff2f4f4f);
		public static readonly Color DarkSlateGrey = new Color (0xff2f4f4f);
		public static readonly Color DarkTurquoise = new Color (0xff00ced1);
		public static readonly Color DarkViolet = new Color (0xff9400d3);
		public static readonly Color DeepPink = new Color (0xffff1493);
		public static readonly Color DeepSkyBlue = new Color (0xff00bfff);
		public static readonly Color DimGray = new Color (0xff696969);
		public static readonly Color DimGrey = new Color (0xff696969);
		public static readonly Color DodgerBlue = new Color (0xff1e90ff);
		public static readonly Color Firebrick = new Color (0xffb22222);
		public static readonly Color FloralWhite = new Color (0xfffffaf0);
		public static readonly Color ForestGreen = new Color (0xff228b22);
        public static readonly Color Fuchsia = new Color (0xffff00ff);
        public static readonly Color Gainsboro = new Color (0xffdcdcdc);
        public static readonly Color GhostWhite = new Color (0xfff8f8ff);
        public static readonly Color Gold = new Color (0xffffd700);
        public static readonly Color Goldenrod = new Color (0xffdaa520);
		public static readonly Color Gray = new Color (0xff808080);
		public static readonly Color Grey = new Color (0xff808080);
		public static readonly Color Green = new Color (0xff008000);
        public static readonly Color GreenYellow = new Color (0xffadff2f);
		public static readonly Color Honeydew = new Color (0xfff0fff0);
		public static readonly Color HotPink = new Color (0xffff69b4);
		public static readonly Color IndianRed = new Color (0xffcd5c5c);
		public static readonly Color Indigo = new Color (0xff4b0082);
		public static readonly Color Ivory = new Color (0xfffffff0);
		public static readonly Color Khaki = new Color (0xfff0e68c);
		public static readonly Color Lavender = new Color (0xffe6e6fa);
		public static readonly Color LavenderBlush = new Color (0xfffff0f5);
		public static readonly Color LawnGreen = new Color (0xff7cfc00);
		public static readonly Color LemonChiffon = new Color (0xfffffacd);
		public static readonly Color LightBlue = new Color (0xffadd8e6);


            {"lightcoral", "#f08080"},
            {"lightcyan", "#e0ffff"},
            {"lightgoldenrodyellow", "#fafad2"},
            {"lightgray", "#d3d3d3"},
            {"lightgreen", "#90ee90"},
            {"lightgrey", "#d3d3d3"},
            {"lightpink", "#ffb6c1"},
            {"lightsalmon", "#ffa07a"},
            {"lightseagreen", "#20b2aa"},
            {"lightskyblue", "#87cefa"},
            {"lightslategray", "#778899"},
            {"lightslategrey", "#778899"},
            {"lightsteelblue", "#b0c4de"},
            {"lightyellow", "#ffffe0"},
            {"lime", "#00ff00"},
            {"limegreen", "#32cd32"},
            {"linen", "#faf0e6"},
            {"magenta", "#ff00ff"},
            {"maroon", "#800000"},
            {"mediumaquamarine", "#66cdaa"},
            {"mediumblue", "#0000cd"},
            {"mediumorchid", "#ba55d3"},
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
	}
}

