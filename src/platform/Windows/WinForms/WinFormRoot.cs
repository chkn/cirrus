using System;
using System.Windows.Forms;

using Cirrus;
using Cirrus.UI;
using Cirrus.Events;

namespace Cirrus.Windows {
	
	public class RootForm : Form {
		
		public GdiPlusCanvas Canvas { get; private set; }
		
		public RootForm (string title, int width, int height)
		{
			Text = title;
			Width = width;
			Height = height;
			Canvas = new GdiPlusCanvas (this);
			
			SetStyle (ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
			SetStyle (ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, false);
			
			Resize += OnResize;
		}
		
		private void OnResize (object o, EventArgs e)
		{
			//Canvas.Resize (this.Width, this.Height);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			Canvas.Render (e.Graphics);
		}
	}
	
	public class WinFormRoot : RootWidget {
		
		public RootForm Peer { get; private set; }
		
		public WinFormRoot (RootForm form)
			: base (form.Canvas)
		{
			this.Peer = form;
			SetupEventHandlers ();
		}
		
		public override Future<BoundsChange> OnBoundsChange ()
		{
			var peerChange = Future.ForAny (Future<EventArgs>.FromEvent (f => Peer.Resize += f, f => Peer.Resize -= f),
				                            Future<EventArgs>.FromEvent (f => Peer.Move += f, f => Peer.Move -= f))
			                       .Then (() => Bounds.UpdateBounds (Peer.Left, Peer.Top, Peer.Width, Peer.Height));
			
			var userChange = Bounds.On<BoundsChange> ().Then (bc => {
				Peer.SetBounds ((int)bc.Bounds.X, (int)bc.Bounds.Y, (int)bc.Bounds.Width, (int)bc.Bounds.Height);
				return bc;
			});
			
			return peerChange | userChange;
		}
		
		public override Future<MouseMove> OnMouseMove ()
		{
			throw new NotImplementedException ();
		}

		public override Future<MouseDown> OnMouseDown ()
		{
			throw new NotImplementedException ();
		}

		public override Future<MouseUp> OnMouseUp ()
		{
			throw new NotImplementedException ();
		}	
		
	}
}

