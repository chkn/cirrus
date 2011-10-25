using System;
using Cirrus;

namespace Cirrus.Samples.BouncingBall {
	
	public static class MainClass {
		
		[EntryPoint]
		public static Future Init ()
		{
			var canvas = Platform.Current.MainCanvas;
			var ctx = canvas.GetContext ("2d");
			
			
			ctx.FillStyle = "blue";
			int x = 0;
			for (int y = 0; y < canvas.Height; y++) {
			
				ctx.ClearRect (0, 0, canvas.Width, canvas.Height);
				ctx.FillRect (x, y, 10, 10);
				
				Thread.Sleep (150);
				x++;
			}
			
			return null;
		}
		
	}
}

