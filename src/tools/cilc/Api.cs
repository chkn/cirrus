/*
	Api.cs: .NET -> Foreign API mapping
  
	Copyright (c) 2011 Alexander Corrado
  
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Collections.Generic;

using Mono.Cecil;

namespace Cirrus.Tools.Cilc {
	
	public static class Api {

		private static Dictionary<string,Implementation> cache;
		private static Dictionary<string,Implementation> Cache {
			get {
				if (cache == null)
					cache = new Dictionary<string, Implementation> ();
				return cache;
			}
		}
		
		private static List<string> search_paths;
		private static List<string> SearchPaths {
			get {
				if (search_paths == null)
					search_paths = new List<string> ();
				return search_paths;
			}
		}
		
		public static bool Load (string reference)
		{
			var basepath = reference;
			
			// try to find associated api
			if (!Directory.Exists (basepath)) {
				var lastDot = reference.LastIndexOf ('.');
				if (lastDot > 0)
					basepath = reference.Substring (0, lastDot) + ".api";
				
				if (lastDot <= 0 || !Directory.Exists (basepath))
					basepath = Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), Path.GetFileName (basepath));
			}
			
			if (!Directory.Exists (basepath))
				return false;
			
			SearchPaths.Add (basepath);
			
			// Load all files in basepath by default
			foreach (var file in Directory.GetFiles (basepath)) {
				if (!file.EndsWith (".xml"))
					continue;
				
				foreach (var impl in Implementation.FromXDocument (XDocument.Load (file)))
					Cache.Add (impl.Name, impl);
			}
			return true;
		}
		
		public static Implementation GetInvokeOptions (this MethodReference method)
		{
			return method.DeclaringType.GetImplementationOptions ().ForMethodInvoke (method);
		}
		
		public static Implementation GetImplementationOptions (this TypeReference type)
		{
			Implementation impl = null;
			
			if (type is GenericInstanceType)
				type = type.GetElementType ();
			
			// First, check cache.
			if (Cache.TryGetValue (type.FullName, out impl))
				return impl;
			
			// Second, check for api file.
			foreach (var basepath in SearchPaths) {
			
				var apifile = Path.Combine (basepath, type.FullName.Replace ('.', Path.DirectorySeparatorChar) + ".xml");
				if (File.Exists (apifile)) {
					impl = Implementation.FromXDocument (XDocument.Load (apifile)).SingleOrDefault (i => i.Name == type.FullName);
					if (impl != null)
						break;
				}
			}
			
			// Last but not least, return default.
			if (impl == null)
				return new Implementation (type.FullName);
			
			Cache.Add (type.FullName, impl);
			return impl;
		}
		
	}
}

