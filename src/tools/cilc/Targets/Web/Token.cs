using System;

namespace Cirrus.Tools.Cilc.Targets.Web {
	
	public struct Token {
		
		public string JavaScript;
		public byte CwpCode;
		
		public Token (string Js, byte Cwp)
		{
			this.JavaScript = Js;
			this.CwpCode = Cwp;
		}
		
		public static implicit operator Token(string id)
		{
			return new Token (id, 0);	
		}
	}
}

