/*
	Based on Utility.cs from the SLCanvas project
	http://slcanvas.codeplex.com/
	
	Microsoft Public License (Ms-PL)
	
	This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
	
	1. Definitions
	The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
	A "contribution" is the original software, or any additions or changes to the software.
	A "contributor" is any person that distributes its contribution under this license.
	"Licensed patents" are a contributor's patent claims that read directly on its contribution.
	
	2. Grant of Rights
	(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
	(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
	
	3. Conditions and Limitations
	(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
	(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
	(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
	(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
	(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Cirrus.Gfx;

namespace Cirrus.Codec.Css.Types {

	public class CssColor : CssType<Color> {
		
		private static readonly Regex colorHex6Regex = new Regex(@"#[0-9a-fA-F]{6}");
        private static readonly Regex colorHex3Regex = new Regex(@"#[0-9a-fA-F]{3}");
        private static readonly Regex colorRgbRegex = new Regex(@"rgb\(\s*(?<r>\d+)\s*,\s*(?<g>\d+)\s*,\s*(?<b>\d+)\s*\)");
        private static readonly Regex colorRgbaRegex = new Regex(@"rgba\(\s*(?<r>\d+)\s*,\s*(?<g>\d+)\s*,\s*(?<b>\d+)\s*,\s*(?<a>.+)\s*\)");
		// From http://www.w3.org/TR/css3-color/
        private static readonly Dictionary<string, string> colorToHex6 = new Dictionary<string, string>
        {
            {"aliceblue", "#f0f8ff"},
            {"antiquewhite", "#faebd7"},
            {"aqua", "#00ffff"},
            {"aquamarine", "#7fffd4"},
            {"azure", "#f0ffff"},
            {"beige", "#f5f5dc"},
            {"bisque", "#ffe4c4"},
            {"black", "#000000"},
            {"blanchedalmond", "#ffebcd"},
            {"blue", "#0000ff"},
            {"blueviolet", "#8a2be2"},
            {"brown", "#a52a2a"},
            {"burlywood", "#deb887"},
            {"cadetblue", "#5f9ea0"},
            {"chartreuse", "#7fff00"},
            {"chocolate", "#d2691e"},
            {"coral", "#ff7f50"},
            {"cornflowerblue", "#6495ed"},
            {"cornsilk", "#fff8dc"},
            {"crimson", "#dc143c"},
            {"cyan", "#00ffff"},
            {"darkblue", "#00008b"},
            {"darkcyan", "#008b8b"},
            {"darkgoldenrod", "#b8860b"},
            {"darkgray", "#a9a9a9"},
            {"darkgreen", "#006400"},
            {"darkgrey", "#a9a9a9"},
            {"darkkhaki", "#bdb76b"},
            {"darkmagenta", "#8b008b"},
            {"darkolivegreen", "#556b2f"},
            {"darkorange", "#ff8c00"},
            {"darkorchid", "#9932cc"},
            {"darkred", "#8b0000"},
            {"darksalmon", "#e9967a"},
            {"darkseagreen", "#8fbc8f"},
            {"darkslateblue", "#483d8b"},
            {"darkslategray", "#2f4f4f"},
            {"darkslategrey", "#2f4f4f"},
            {"darkturquoise", "#00ced1"},
            {"darkviolet", "#9400d3"},
            {"deeppink", "#ff1493"},
            {"deepskyblue", "#00bfff"},
            {"dimgray", "#696969"},
            {"dimgrey", "#696969"},
            {"dodgerblue", "#1e90ff"},
            {"firebrick", "#b22222"},
            {"floralwhite", "#fffaf0"},
            {"forestgreen", "#228b22"},
            {"fuchsia", "#ff00ff"},
            {"gainsboro", "#dcdcdc"},
            {"ghostwhite", "#f8f8ff"},
            {"gold", "#ffd700"},
            {"goldenrod", "#daa520"},
            {"gray", "#808080"},
            {"green", "#008000"},
            {"greenyellow", "#adff2f"},
            {"grey", "#808080"},
            {"honeydew", "#f0fff0"},
            {"hotpink", "#ff69b4"},
            {"indianred", "#cd5c5c"},
            {"indigo", "#4b0082"},
            {"ivory", "#fffff0"},
            {"khaki", "#f0e68c"},
            {"lavender", "#e6e6fa"},
            {"lavenderblush", "#fff0f5"},
            {"lawngreen", "#7cfc00"},
            {"lemonchiffon", "#fffacd"},
            {"lightblue", "#add8e6"},
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
            {"yellowgreen", "#9acd32"},
        };
			
		public override string Format (Color color)
		{
			if (byte.MaxValue == color.A)
                // Use 6-character form
                return string.Format(CultureInfo.InvariantCulture, "#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
            else
                // Use 8-character form
                return string.Format(CultureInfo.InvariantCulture, "rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, color.A / ((double)byte.MaxValue));
		}
		
		// Attempt to parse a color by the various allowed forms
        public override bool TryParse (string cssColor, out Color result)
        {
			result = default (Color);
			string hexColor;
			Match match;
			
			if (colorToHex6.TryGetValue (cssColor.ToLower (CultureInfo.InvariantCulture), out hexColor))
				cssColor = hexColor;
			
            if (colorHex6Regex.Match (cssColor).Success)
            {
                result.R = byte.Parse(cssColor.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				result.G = byte.Parse(cssColor.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				result.B = byte.Parse(cssColor.Substring(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				result.A = byte.MaxValue;
            }
            else if (colorHex3Regex.Match (cssColor).Success)
            {
                result.R = (byte)(byte.Parse(cssColor.Substring(1, 1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) << 4);
				result.G = (byte)(byte.Parse(cssColor.Substring(2, 1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) << 4);
				result.B = (byte)(byte.Parse(cssColor.Substring(3, 1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) << 4);
				result.A = byte.MaxValue;
            }
            else if ((match = colorRgbRegex.Match (cssColor)).Success)
            {
                result.R = byte.Parse(match.Groups["r"].Value, CultureInfo.InvariantCulture);
				result.G = byte.Parse(match.Groups["g"].Value, CultureInfo.InvariantCulture);
				result.B = byte.Parse(match.Groups["b"].Value, CultureInfo.InvariantCulture);
				result.A = byte.MaxValue;
            }
            else if ((match = colorRgbaRegex.Match (cssColor)).Success)
            {
				result.R = byte.Parse(match.Groups["r"].Value, CultureInfo.InvariantCulture);
				result.G = byte.Parse(match.Groups["g"].Value, CultureInfo.InvariantCulture);
				result.B = byte.Parse(match.Groups["b"].Value, CultureInfo.InvariantCulture);
				result.A = (byte)(byte.MaxValue * double.Parse(match.Groups["a"].Value, CultureInfo.InvariantCulture));
            }
            else
            {
                return false;
            }
            return true;
        }
	}
}

