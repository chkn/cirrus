/*
	Http.cs: Http requests
  
	Copyright (c) 2010 Alexander Corrado
  
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

using Cirrus;

using System.IO;
using System.Net;
using System.Text;

namespace Cirrus.Web {
	
	public static class Http {
		
		public struct RequestArgs {
			public string URL;
			public string Method;
			public string PostData;
			public bool Binary;
			//public ? Range;
		}
		
		public static Future<Response> Request(string URL)
		{
			return Request (new RequestArgs { URL = URL });	
		}
		
		public static Future<Response> Request (RequestArgs args)
		{
			if (args.URL == null)
				throw new System.ArgumentNullException ("URL");
			
			var req = WebRequest.Create (args.URL);
			req.Method = args.Method ?? "GET";
			
			
			//FIXME: Use the rest of args...
			
			var resp = Future<WebResponse>.FromApm (ac => req.BeginGetResponse (ac, null), req.EndGetResponse).Wait ();
			return new Response (args, resp);
		}
		
		public sealed class Response {
			
			public RequestArgs Request { get; private set; }
			
			private WebResponse response;
			private string responseText;
			
			internal Response (RequestArgs request, WebResponse response)
			{
				this.Request = request;
				this.response = response;
			}
			
			public string ResponseText {
				get {
					if (responseText == null) {
						var	stream = response.GetResponseStream ();
						var reader = new StreamReader (stream, Encoding.UTF8);
						responseText = reader.ReadToEnd ();
						reader.Dispose ();
						stream.Dispose ();
						response.Close ();
					}
					
					return responseText;
				}
			}
		}
	}
}

