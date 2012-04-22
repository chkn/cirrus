using System;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Cirrus.Gfx;
using Color = Cirrus.Gfx.Imaging.Formats.Pixel32bpp;

namespace Cirrus.Codec.Css.Types {

	public class CssColor : CssType<Color> {
		
		private static readonly Regex colorHex6Regex = new Regex(@"#[0-9a-fA-F]{6}");
        private static readonly Regex colorHex3Regex = new Regex(@"#[0-9a-fA-F]{3}");
        private static readonly Regex colorRgbRegex = new Regex(@"rgb\(\s*(?<r>\d+)\s*,\s*(?<g>\d+)\s*,\s*(?<b>\d+)\s*\)");
        private static readonly Regex colorRgbaRegex = new Regex(@"rgba\(\s*(?<r>\d+)\s*,\s*(?<g>\d+)\s*,\s*(?<b>\d+)\s*,\s*(?<a>.+)\s*\)");

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
			Match match;
			result = new Color ();
			
            if (colorHex6Regex.Match (cssColor).Success) {
                result.R = byte.Parse(cssColor.Substring(1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				result.G = byte.Parse(cssColor.Substring(3, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				result.B = byte.Parse(cssColor.Substring(5, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				result.A = byte.MaxValue;
				return true;
            }

			if (colorHex3Regex.Match (cssColor).Success) {
                result.R = (byte)(byte.Parse(cssColor.Substring(1, 1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) << 4);
				result.G = (byte)(byte.Parse(cssColor.Substring(2, 1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) << 4);
				result.B = (byte)(byte.Parse(cssColor.Substring(3, 1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) << 4);
				result.A = byte.MaxValue;
				return true;
            }

            if ((match = colorRgbRegex.Match (cssColor)).Success) {
                result.R = byte.Parse(match.Groups["r"].Value, CultureInfo.InvariantCulture);
				result.G = byte.Parse(match.Groups["g"].Value, CultureInfo.InvariantCulture);
				result.B = byte.Parse(match.Groups["b"].Value, CultureInfo.InvariantCulture);
				result.A = byte.MaxValue;
				return true;
            }

            else if ((match = colorRgbaRegex.Match (cssColor)).Success) {
				result.R = byte.Parse(match.Groups["r"].Value, CultureInfo.InvariantCulture);
				result.G = byte.Parse(match.Groups["g"].Value, CultureInfo.InvariantCulture);
				result.B = byte.Parse(match.Groups["b"].Value, CultureInfo.InvariantCulture);
				result.A = (byte)(byte.MaxValue * double.Parse(match.Groups["a"].Value, CultureInfo.InvariantCulture));
				return true;
            }

			var field = typeof (Color).GetField (cssColor, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);
			if (field != null) {
				result = (Color)field.GetValue (null);
				return true;
			}

			return false;
        }
	}
}

