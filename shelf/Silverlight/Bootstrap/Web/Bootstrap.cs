/*
 * Cirrus Silverlight-based web bootstrapper.
 * Copyright 2010 Alexander Corrado.
 */

using System;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cirrus.Silverlight {
	public class Bootstrap : Application {
		public Bootstrap ()
		{
			this.Startup += OnStartup;
		}

		void OnStartup (object sender, StartupEventArgs e)
		{
			ScriptObject boundingRect = (ScriptObject)HtmlPage.Plugin.Invoke("getBoundingClientRect");
			int width = Convert.ToInt32(boundingRect.GetProperty("width"));
			if (width == 0)
				width = Convert.ToInt32(boundingRect.GetProperty("right")) - Convert.ToInt32(boundingRect.GetProperty("left"));
			int height = Convert.ToInt32(boundingRect.GetProperty("height"));
			if (height == 0)
				height = Convert.ToInt32(boundingRect.GetProperty("bottom")) - Convert.ToInt32(boundingRect.GetProperty("top"));
			
			Canvas silverlightCanvas = new Canvas();
			this.RootVisual = silverlightCanvas;
			
			CirrusCanvas canvas = new CirrusCanvas(silverlightCanvas, width, height);
			HtmlPage.RegisterScriptableObject("canvas", canvas);
		}
	}
}

