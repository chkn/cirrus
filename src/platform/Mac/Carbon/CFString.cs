using System;
using System.Runtime.InteropServices;

namespace Cirrus.Mac {
	public class CFString : IDisposable {
		public IntPtr Native { get; private set; }
		
		public static implicit operator IntPtr(CFString cfstr)
		{
			return cfstr.Native;	
		}
		public static implicit operator CFString(string str)
		{
			return new CFString { Native = CFStrMake (str) };
		}
		
		public void Dispose ()
		{
			CFRelease (Native);
		}
		
		[DllImport (Carbon.LIB, EntryPoint = "__CFStringMakeConstantString")]
		private extern static IntPtr CFStrMake (string cString);
		[DllImport(Carbon.LIB)]
		private extern static int CFRelease (IntPtr wHnd);
	}
}

