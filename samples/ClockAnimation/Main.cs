/*
	This sample is based on Mozilla's HTML5 Canvas Animation Example 2 at:
		https://developer.mozilla.org/en/Canvas_tutorial/Basic_animations
*/

using System;
using Cirrus;
using Cirrus.Gfx;
using Cirrus.Gfx.Drawing;

namespace Cirrus.Samples.ClockAnimation {
	
	public static class MainClass {
		
		[EntryPoint]
		public static void Init ()
		{
			
			Platform.Current.Root.Renderer = (layer, rect, ctx) =>
			{
				
				while (true) {
					ctx.Save ();
					ctx.ClearRect(0,0,rect.Width,rect.Height);
					ctx.Translate(75,75);
					ctx.Scale(0.4,0.4);
					ctx.Rotate(-Math.PI/2);
					ctx.StrokeStyle = "#000000";
					ctx.FillStyle = "#FFFFFF";
					ctx.LineWidth = 8;
					ctx.LineCap = LineCap.Round;
					
					// Hour marks
					ctx.Save();
					for (var i=0;i<12;i++){
						ctx.BeginPath();
						ctx.Rotate(Math.PI/6);
						ctx.MoveTo(100,0);
						ctx.LineTo(120,0);
						ctx.Stroke();
					}
					ctx.Restore();
					
					// Minute marks
					ctx.Save();
					ctx.LineWidth = 5;
					for (var i=0;i<60;i++){
						if (i%5!=0) {
						  ctx.BeginPath();
						  ctx.MoveTo(117,0);
						  ctx.LineTo(120,0);
						  ctx.Stroke();
						}
						ctx.Rotate(Math.PI/30);
					}
					ctx.Restore();
					
					var now = System.DateTime.Now;
					var sec = now.Second;
					var min = now.Minute;
					var hr  = now.Hour;
					hr = hr>=12 ? hr-12 : hr;
					
					ctx.FillStyle = "#000000";
					
					// write Hours
					ctx.Save();
					ctx.Rotate( hr*(Math.PI/6) + (Math.PI/360)*min + (Math.PI/21600)*sec );
					ctx.LineWidth = 14;
					ctx.BeginPath();
					ctx.MoveTo(-20,0);
					ctx.LineTo(80,0);
					ctx.Stroke();
					ctx.Restore();
					
					// write Minutes
					ctx.Save();
					ctx.Rotate( (Math.PI/30)*min + (Math.PI/1800)*sec );
					ctx.LineWidth = 10;
					ctx.BeginPath();
					ctx.MoveTo(-28,0);
					ctx.LineTo(112,0);
					ctx.Stroke();
					ctx.Restore();
					
					// Write seconds
					ctx.Save();
					ctx.Rotate(sec * Math.PI/30);
					ctx.StrokeStyle = "#D40000";
					ctx.FillStyle = "#D40000";
					ctx.LineWidth = 6;
					ctx.BeginPath();
					ctx.MoveTo(-30,0);
					ctx.LineTo(83,0);
					ctx.Stroke();
					ctx.BeginPath();
					ctx.Arc(0,0,10,0,Math.PI*2,true);
					ctx.Fill();
					ctx.BeginPath();
					ctx.Arc(95,0,10,0,Math.PI*2,true);
					ctx.Stroke();
					ctx.FillStyle = "#555";
					ctx.Arc(0,0,3,0,Math.PI*2,true);
					ctx.Fill();
					ctx.Restore();
					
					ctx.BeginPath();
					ctx.LineWidth = 14;
					ctx.StrokeStyle = "#325FA2";
					ctx.Arc(0,0,142,0,Math.PI*2,true);
					ctx.Stroke();
					
					ctx.Restore ();
					Future.MillisecondsFromNow (1000).Wait ();
				}
			};
			
			Platform.Current.Root.Render ();
		}
		
	
	}
}

