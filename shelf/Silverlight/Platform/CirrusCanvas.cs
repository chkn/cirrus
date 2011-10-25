using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cirrus.Silverlight {
	
#if WEB
	using System.Windows.Browser;
	public class CirrusCanvas {
#else
	using Cirrus.Gfx;
	public class CirrusCanvas : ICanvas {
#endif
		
		internal Canvas canvas;
		private CanvasRenderingContext2D ctx;
		
		public CirrusCanvas (Canvas canvas, int width, int height)
		{
			this.canvas = canvas;
			this.canvas.Width = width;
			this.canvas.Height = height;
				
			this.ctx = new CanvasRenderingContext2D(this);
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "getContext")]
		public CanvasRenderingContext2D GetContext (string id)
#else
		public ICanvasRenderingContext2D GetContext (string id)
#endif
		{
			if (id == "2d")
				return ctx;
			return null;
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "width")]
#endif
		public int Width {
			get { return (int)canvas.Width; }
			set { canvas.Width = value; }	
		}
		
#if WEB
		[ScriptableMember (ScriptAlias = "height")]
#endif
		public int Height {
			get { return (int)canvas.Height; }
			set { canvas.Height = value; }
		}
	}
}

