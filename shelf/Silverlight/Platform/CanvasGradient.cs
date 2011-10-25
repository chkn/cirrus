using System.Windows.Media;

using Cirrus.Codec.Css.Parsers;

namespace Cirrus.Silverlight {

#if WEB
	using System.Windows.Browser;
	public class CanvasGradient {
#else
	using Cirrus.Gfx;
    public class CanvasGradient : ICanvasGradient {
#endif
        public CanvasGradient()
        {
            OffsetMinimum = 0;
            OffsetMultiplier = 1;
        }
			
#if WEB
		[ScriptableMember (ScriptAlias = "addColorStop")]
#endif
        public void AddColorStop(double offset, string cssColor)
        {
			byte a, r, g, b;
            if ((0.0 <= offset) && (offset <= 1.0) && CssColor.TryParse (cssColor, out r, out g, out b, out a))
            {
				var color = Color.FromArgb (a, r, g, b);
                GradientBrush.GradientStops.Add(new GradientStop { Offset = OffsetMinimum + (offset * OffsetMultiplier), Color = color });
            }
        }

        internal GradientBrush GradientBrush { get; set; }

        // OffsetMinimum/OffsetMultiplier are used by createRadialGradient to
        // more easily implement its starting/ending radius behavior
        internal double OffsetMinimum { get; set; }
        internal double OffsetMultiplier { get; set; }
    }
}
