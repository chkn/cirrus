#if WEB
using System.Windows.Browser;
#endif
using System.Windows.Media;

namespace Cirrus.Silverlight {
	
    // Implements the CanvasPattern interface
#if WEB
    [ScriptableType]
	public class CanvasPattern
#else
    public class CanvasPattern : Cirrus.Gfx.ICanvasPattern
#endif
    {
        internal ImageBrush ImageBrush { get; set; }
    }
}
