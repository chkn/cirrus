using System;
using System.Runtime.InteropServices;

namespace Cirrus.Mac {
	internal static class Carbon {
		public const string LIB = "/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon";
		
		public static void Check (this int osErr, string method)
		{
			if (osErr != 0)
				throw new SystemException ("Call to '" + method + "' returned unexpected OS error code: " + osErr);
		}
		
		#region Process/Init
		
		[DllImport (LIB)] public extern static int GetCurrentProcess (ref Carbon.ProcessSerialNumber psn);
		[DllImport (LIB)] public extern static int TransformProcessType (ref Carbon.ProcessSerialNumber psn, uint type);
		[DllImport (LIB)] public extern static int SetFrontProcess (ref Carbon.ProcessSerialNumber psn);
		
		public struct ProcessSerialNumber {
		 	public UInt32 highLongOfPSN;
		    public UInt32 lowLongOfPSN;
		}
		
		#endregion
		
		#region Window Management
		
		[DllImport (LIB)] public extern static int CreateNewWindow (WindowClass klass, WindowAttributes attributes, ref Rect r, ref IntPtr window);
		[DllImport (LIB)] public extern static int DisposeWindow (IntPtr wHnd);
		[DllImport (LIB)] public extern static int ShowWindow (IntPtr wHnd);
		[DllImport (LIB)] public extern static int SetWindowTitleWithCFString (IntPtr hWnd, IntPtr titleCFStr);
		
		public struct Rect {
			public short top;
			public short left;
			public short bottom;
			public short right;
			
			public Rect (short left, short top, short right, short bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}
		}
		
		#endregion
		
		#region HIView
		
		[DllImport (LIB)] public extern static IntPtr HIViewGetRoot (IntPtr hWnd);
		[DllImport (LIB)] public extern static int HIViewFindByID (IntPtr inWnd, HIViewID id, out IntPtr ctrl);
		[DllImport (LIB)] public static extern int HIViewSetNeedsDisplay (IntPtr inView, int inNeedsDisplay);
		[DllImport (LIB)] public static extern int HIViewSetNeedsDisplayInRect (IntPtr inView, ref CoreGraphics.CGRect inRect, int inNeedsDisplay);
		[DllImport (LIB)] public static extern int HIViewSetVisible (IntPtr inView, int visible);
		[DllImport (LIB)] public static extern int HIViewAddSubview(IntPtr parent, IntPtr sub);
		[DllImport (LIB)] public static extern int HIViewGetBounds (IntPtr inView, ref CoreGraphics.CGRect outBounds);
		[DllImport (LIB)] public static extern int HIViewChangeFeatures (IntPtr view, ulong toAdd, ulong toClear);
		
		[DllImport (LIB)] public static extern int HIObjectCreate(IntPtr cfstrClassID, IntPtr inConstructData, out IntPtr outObject);
		[DllImport (LIB)] public static extern int CreateEvent(IntPtr inAllocator, EventClass inClassID, uint inKind, double inWhen, EventAttributes inAttributes, out IntPtr outEvent);
		[DllImport (LIB)] public static extern void ReleaseEvent (IntPtr evt);
		
		[DllImport (LIB)] public static extern double GetCurrentEventTime();
		
		public struct HIViewID {
			public uint kind;
			public uint id;
			
			public HIViewID (uint klass, uint id)
			{
				this.kind = klass;
				this.id = id;
			}
		}
		
		#endregion
		
		
		#region Events
		[DllImport (LIB)]
		public static extern IntPtr GetApplicationEventTarget ();
		[DllImport (LIB)]
		public static extern IntPtr GetControlEventTarget (IntPtr inControl);
		
		[DllImport (LIB)]
		public static extern IntPtr GetMainEventLoop ();
		[DllImport (LIB)]
		public static extern IntPtr GetMainEventQueue ();
		
		[DllImport (LIB)]
		public static extern IntPtr GetEventDispatcherTarget ();
		[DllImport (LIB)]
		public static extern int SendEventToEventTarget (IntPtr evt, IntPtr target);
		
		[DllImport (LIB)]
		public static extern int ReceiveNextEvent (int count, IntPtr evtList, double timeout, [MarshalAs (UnmanagedType.U1)] bool blnOwnEvent, out IntPtr evtRef);
		
		[DllImport (LIB)]
		public static extern int PostEventToQueue (IntPtr inQueue, IntPtr inEvent, EventPriority priority);
		
		[DllImport (LIB)]
		public static extern int InstallEventHandler (IntPtr target, EventDelegate handler, uint count,
		                                               [MarshalAs (UnmanagedType.LPArray)] EventTypeSpec [] types, IntPtr user_data, out IntPtr handlerRef);
		[DllImport (LIB)]
		public static extern int RemoveEventHandler (IntPtr handler);
		
		[DllImport (LIB)]
		public static extern int CallNextEventHandler (IntPtr inCallRef, IntPtr inEvent);
		
		[DllImport (LIB)]
		public static extern int InstallEventLoopTimer (IntPtr inEventLoop, double secondsBeforeFirst, double intervalInSeconds,
		                                                EventTimerDelegate inTimerProc, IntPtr inTimerData, out IntPtr outTimer);
		
		[DllImport (LIB)]
		public static extern int SetEventLoopTimerNextFireTime (IntPtr inTimer, double inNextFire);
		
		[DllImport (LIB)]
		public static extern int RemoveEventLoopTimer (IntPtr inTimer);
		
		[DllImport (LIB)]
		public static extern int GetEventParameter (IntPtr eventRef, Carbon.EventParameterName name, Carbon.EventParameterType desiredType,
		                                                    IntPtr actualType, uint size, IntPtr outSize, out IntPtr outPtr);
		[DllImport (LIB)]
		public static extern int SetEventParameter (IntPtr eventRef, Carbon.EventParameterName name, Carbon.EventParameterType desiredType,
		                                                    uint size, ref CoreGraphics.CGRect data);

		
		[DllImport(LIB)]
		public extern static void RunApplicationEventLoop ();
		
		public delegate EventHandlerStatus EventDelegate (IntPtr callRef, IntPtr eventRef, IntPtr userData);
		public delegate void EventTimerDelegate (IntPtr timer, IntPtr userData);
		
		public enum EventHandlerStatus //this is an OSStatus
		{
			Handled = 0,
			NotHandled = -9874,
			UserCancelled = -128,
		}
		
		[StructLayout(LayoutKind.Sequential, Pack = 2)]
		public struct EventTypeSpec
		{
			public EventClass EventClass;
			public uint EventKind;
		}
		
		public enum EventClass : uint
		{
			Mouse = 1836021107, // 'mous'
			Keyboard = 1801812322, // 'keyb'
			TextInput = 1952807028, // 'text'
			Application = 1634758764, // 'appl'
			RemoteAppleEvent = 1701867619,  //'eppc' //remote apple event?
			Menu = 1835363957, // 'menu'
			Window = 2003398244, // 'wind'
			Control = 1668183148, // 'cntl'
			Command = 1668113523, // 'cmds'
			Tablet = 1952607348, // 'tblt'
			Volume = 1987013664, // 'vol '
			Appearance = 1634758765, // 'appm'
			Service = 1936028278, // 'serv'
			Toolbar = 1952604530, // 'tbar'
			ToolbarItem = 1952606580, // 'tbit'
			Accessibility = 1633903461, // 'acce'
			HIObject = 1751740258, // 'hiob'
			AppleEvent = 1634039412, // 'aevt'
			Internet = 1196773964, // 'GURL'
			KWIN = 1264011598
		}
		
		public enum EventAttributes : uint {
  			kEventAttributeNone           = 0,
  			kEventAttributeUserEvent      = 1 << 0,
  			kEventAttributeMonitored      = 1 << 3
		};
		public enum EventParameterName : uint
		{
			DirectObject = 757935405, // '----'
			AEPosition = 1802530675, // 'kpos'
			CGContextRef = 1668183160, // 'cntx'
			Bounds = 1651471726, // 'boun'
		}
		
		public enum EventParameterType : uint
		{
			HICommand = 1751346532, // 'hcmd'
			MenuRef = 1835363957, // 'menu'
			WindowRef = 2003398244, // 'wind'
			ControlRef = 1668575852, // 'ctrl'
			Char = 1413830740, // 'TEXT'
			UInt32 = 1835100014, // 'magn'
			UnicodeText = 1970567284, // 'utxt'
			AEList = 1818850164, // 'list'
			WildCard = 707406378, // '****'
			FSRef = 1718841958, // 'fsrf' 
			CGContextRef = 1668183160, // 'cntx'
			HIRect = 1751741027, // 'hirc'
		}
		
		public enum EventPriority : short
		{
			kEventPriorityLow = 0,
			kEventPriorityStandard = 1,
			kEventPriorityHigh = 2
		}
		
	#endregion
		
		
		public enum WindowClass : uint {
			kAlertWindowClass = 1,
			kMovableAlertWindowClass = 2,
			kModalWindowClass = 3,
			kMovableModalWindowClass = 4,
			kFloatingWindowClass = 5,
			kDocumentWindowClass = 6,
			kUtilityWindowClass = 8,
			kHelpWindowClass = 10,
			kSheetWindowClass = 11,
			kToolbarWindowClass = 12,
			kPlainWindowClass = 13,
			kOverlayWindowClass = 14,
			kSheetAlertWindowClass = 15,
			kAltPlainWindowClass = 16,
			kDrawerWindowClass = 20,
			kAllWindowClasses = 0xFFFFFFFF
		}
		
		public enum WindowAttributes : uint {
			kWindowNoAttributes = 0,
			kWindowCloseBoxAttribute = (1 << 0),
			kWindowHorizontalZoomAttribute = (1 << 1),
			kWindowVerticalZoomAttribute = (1 << 2),
			kWindowFullZoomAttribute = (kWindowVerticalZoomAttribute | kWindowHorizontalZoomAttribute),
			kWindowCollapseBoxAttribute = (1 << 3),
			kWindowResizableAttribute = (1 << 4),
			kWindowSideTitlebarAttribute = (1 << 5),
			kWindowToolbarButtonAttribute = (1 << 6),
			kWindowUnifiedTitleAndToolbarAttribute = (1 << 7),
			kWindowMetalAttribute = (1 << 8),
			kWindowNoTitleBarAttribute = (1 << 9),
			kWindowTexturedSquareCornersAttribute = (1 << 10),
			kWindowMetalNoContentSeparatorAttribute = (1 << 11),
			kWindowDoesNotCycleAttribute = (1 << 15),
			kWindowNoUpdatesAttribute = (1 << 16),
			kWindowNoActivatesAttribute = (1 << 17),
			kWindowOpaqueForEventsAttribute = (1 << 18),
			kWindowCompositingAttribute = (1 << 19),
			kWindowFrameworkScaledAttribute = (1 << 20),
			kWindowNoShadowAttribute = (1 << 21),
			kWindowCanBeVisibleWithoutLoginAttribute = (1 << 22),
			kWindowAsyncDragAttribute = (1 << 23),
			kWindowHideOnSuspendAttribute = (1 << 24),
			kWindowStandardHandlerAttribute = (1 << 25),
			kWindowHideOnFullScreenAttribute = (1 << 26),
			kWindowInWindowMenuAttribute = (1 << 27),
			kWindowLiveResizeAttribute = (1 << 28),
			kWindowIgnoreClicksAttribute = (1 << 29),
			//kWindowNoConstrainAttribute = (1 << 31),
			kWindowStandardDocumentAttributes = (kWindowCloseBoxAttribute | kWindowFullZoomAttribute | kWindowCollapseBoxAttribute | kWindowResizableAttribute),
			kWindowStandardFloatingAttributes = (kWindowCloseBoxAttribute | kWindowCollapseBoxAttribute)
		}
		
		public enum WindowRegion : ushort {
		  kWindowTitleBarRgn            = 0,
		  kWindowTitleTextRgn           = 1,
		  kWindowCloseBoxRgn            = 2,
		  kWindowZoomBoxRgn             = 3,
		  kWindowDragRgn                = 5,
		  kWindowGrowRgn                = 6,
		  kWindowCollapseBoxRgn         = 7,
		  kWindowTitleProxyIconRgn      = 8,    /* Mac OS 8.5 forward*/
		  kWindowStructureRgn           = 32,
		  kWindowContentRgn             = 33,   /* Content area of the window; empty when the window is collapsed*/
		  kWindowUpdateRgn              = 34,   /* Carbon forward*/
		  kWindowOpaqueRgn              = 35,   /* Mac OS X: Area of window considered to be opaque. Only valid for windows with alpha channels.*/
		  kWindowGlobalPortRgn          = 40,   /* Carbon forward - bounds of the window’s port in global coordinates; not affected by CollapseWindow*/
		  kWindowToolbarButtonRgn       = 41    /* Mac OS X Tiger: the toolbar button area*/
		};
		
		public enum HICoordinateSpace : uint {
			kHICoordSpace72DPIGlobal      = 1,
			kHICoordSpaceScreenPixel      = 2,
			kHICoordSpaceWindow           = 3,
			kHICoordSpaceView             = 4
		};
	}
}

