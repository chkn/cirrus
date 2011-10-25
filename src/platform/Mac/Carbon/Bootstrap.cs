/*
	Bootstrap.cs: Cirrus Client Bootstrapper
  
	Copyright (c) 2010-2011 Alexander Corrado
  
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using Cirrus;


namespace Cirrus.Mac {
	
	// FIXME: Resize canvas when window is resized
	public class PlatformCarbon : Platform {
		public static void Main (string [] args)
		{
			new PlatformCarbon (args);
		}
		
		
		private IntPtr window = IntPtr.Zero;
		private IntPtr hi_view = IntPtr.Zero;
		
		private Platform.DispatchThread dispatch;
		
		public PlatformCarbon (string [] args) : base (args)
		{
			// init
			var psn = new Carbon.ProcessSerialNumber ();
			Carbon.GetCurrentProcess (ref psn).Check ("GetCurrentProcess");
			Carbon.TransformProcessType (ref psn, 1).Check ("TransformProcessType");
			Carbon.SetFrontProcess (ref psn).Check ("SetFrontProcess");
			
			// ensure native delegates are pinned
			paintDelegate = new Carbon.EventDelegate (HandlePaint);
			runDelegate = new Carbon.EventDelegate (HandleRun);
			var pinPaint = GCHandle.Alloc (paintDelegate, GCHandleType.Pinned);
			var pinRun = GCHandle.Alloc (runDelegate, GCHandleType.Pinned);
			
			
			// setup dispatch
			IntPtr run_handler;
			var dummyEventSpec = new Carbon.EventTypeSpec { EventClass = Carbon.EventClass.KWIN, EventKind = (uint)Carbon.EventClass.KWIN };
			Carbon.InstallEventHandler (Carbon.GetApplicationEventTarget (), runDelegate, 1, new Carbon.EventTypeSpec [] { dummyEventSpec },
										IntPtr.Zero, out run_handler).Check ("InstallEventHandler");
			dispatch = new DispatchThread (Thread.Current);
			dispatch.OnDispatch += Dispatch;
			
			// create window
			var rect = new Carbon.Rect (75, 75, (short)(75+Width), (short)(75+Height));
			Carbon.CreateNewWindow (Carbon.WindowClass.kDocumentWindowClass,
			                        Carbon.WindowAttributes.kWindowCloseBoxAttribute |
			                        	Carbon.WindowAttributes.kWindowCompositingAttribute | 
			                        	Carbon.WindowAttributes.kWindowAsyncDragAttribute |
			                        	Carbon.WindowAttributes.kWindowStandardHandlerAttribute,
			                        ref rect, ref window).Check ("CreateNewWindow");
			
			using (CFString title = Title)
				Carbon.SetWindowTitleWithCFString (window, title).Check ("SetWindowTitleWithCFString");
			
			
			
			// create our content view
			IntPtr init, content;
			var rect2 = new CoreGraphics.CGRect (0, 0, (float)Width, (float)Height);
			
			Carbon.CreateEvent (IntPtr.Zero, Carbon.EventClass.HIObject, 2, Carbon.GetCurrentEventTime (),
			                       Carbon.EventAttributes.kEventAttributeNone, out init).Check ("CreateEvent");
			Carbon.SetEventParameter (init, Carbon.EventParameterName.Bounds, Carbon.EventParameterType.HIRect,
			                          (uint)Marshal.SizeOf (typeof (CoreGraphics.CGRect)), ref rect2).Check ("SetEventParameter");
			
			using (CFString clsid = "com.apple.hiview")
				Carbon.HIObjectCreate (clsid, init, out hi_view).Check ("HIObjectCreate");
			
			// find main window content view and add ours
			Carbon.HIViewFindByID (Carbon.HIViewGetRoot (window), new Carbon.HIViewID ((uint)Carbon.EventClass.Window, 1), out content).Check ("HIViewFindByID");
			Carbon.HIViewAddSubview (content, hi_view).Check ("HIViewAddSubview");
			
			
			
			// prepare draw event handler
			IntPtr paint_handler;
			var drawEventSpec = new Carbon.EventTypeSpec { EventClass = Carbon.EventClass.Control, EventKind = 4 /*draw*/ };
			Carbon.InstallEventHandler (Carbon.GetControlEventTarget (hi_view), paintDelegate, 1,
			                            new Carbon.EventTypeSpec [] { drawEventSpec }, IntPtr.Zero, out paint_handler).Check ("InstallEventHandler"); 


			
			Carbon.HIViewSetVisible (hi_view, -1).Check ("HIViewSetVisible");
			Carbon.ShowWindow (window).Check ("ShowWindow");
			
			Carbon.RunApplicationEventLoop ();
			dispatch.Quit ();
			
			// clean up
			if (MainCanvas != null)
				((CGCanvas)MainCanvas).Dispose ();
			
			Carbon.ReleaseEvent (init);
			Carbon.RemoveEventHandler (run_handler);
			Carbon.RemoveEventHandler (paint_handler);
			
			pinPaint.Free ();
			pinRun.Free ();
		}
		
		private static void Dispatch ()
		{
			IntPtr run_event;
			Carbon.CreateEvent (IntPtr.Zero, Carbon.EventClass.KWIN, (uint)Carbon.EventClass.KWIN, Carbon.GetCurrentEventTime (),
			                    		Carbon.EventAttributes.kEventAttributeNone, out run_event).Check ("CreateEvent");
			
			Carbon.PostEventToQueue (Carbon.GetMainEventQueue (), run_event, Carbon.EventPriority.kEventPriorityStandard).Check ("PostEventToQueue");
			Carbon.ReleaseEvent (run_event);
		}
		
		private Carbon.EventDelegate paintDelegate;
		private Carbon.EventHandlerStatus HandlePaint (IntPtr callRef, IntPtr eventRef, IntPtr userData)
		{
						
			/* FIXME: Handle resize, though we can't release and recreate CGLayer?... what about all the active contexts out there???
			if ((bounds.bottom - bounds.top != Height) ||
			    	(bounds.right - bounds.left != Width) 
				Carbon.CGLayerRelease (cg_layer);
				cg_layer = IntPtr.Zero;
			}
			 */
			
			IntPtr ctx;
			var bounds = default (CoreGraphics.CGRect);
			
			//Carbon.GetEventParameter (eventRef, Carbon.EventParameterName.DirectObject, Carbon.EventParameterType.ControlRef,
			//                          	IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, out hi_view).Check ("GetEventParameter");
			Carbon.GetEventParameter (eventRef, Carbon.EventParameterName.CGContextRef, Carbon.EventParameterType.CGContextRef,
				                        IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, out ctx).Check ("GetEventParameter");
			
			
			// finalize initial init
			if (MainWidget == null) {
				
				Carbon.HIViewGetBounds (hi_view, ref bounds).Check ("HIViewGetBounds");
				
				var canvas = new CGCanvas (hi_view, ctx, bounds.size.width, bounds.size.height);
				MainWidget = new Cirrus.UI.Widget (canvas);
				
				// start user code
				EntryPoint.Invoke (null, null);
				dispatch.Run ();
			}
			
			((CGCanvas)MainCanvas).Render (ctx);
			return Carbon.EventHandlerStatus.Handled;
		}
		
		private Carbon.EventDelegate runDelegate;
		private Carbon.EventHandlerStatus HandleRun (IntPtr callRef, IntPtr eventRef, IntPtr userData)
		{
			Thread.Current.RunSingleIteration ();
			return Carbon.EventHandlerStatus.Handled;
		}
	}
}

