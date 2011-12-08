/*
	CssParser.cs: Parses CSS data
  
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
using System.Collections.Generic;

using Cirrus.Codec.Css.Types;

namespace Cirrus.Codec.Css {
	
	public static class CssParser {
	
		private static Dictionary<Type,object> types = new Dictionary<Type, object> ();
		
		static CssParser ()
		{
			AddType (new CssColor ());
		}
		
		public static CssType<T> GetType<T> ()
		{
			object type;
			if (!types.TryGetValue (typeof (T), out type))
				return null;
			
			return type as CssType<T>;
		}
		
		public static void AddType<T> (CssType<T> type)
		{
			types [typeof (T)] = type;
		}
		
		public static T Parse<T> (string cssExpr)
		{
			return GetType<T> ().Parse (cssExpr);
		}
		
		public static bool TryParse<T> (string cssExpr, out T result)
		{
			result = default (T);
			
			var parser = GetType<T> ();
			if (parser == null)
				return false;
			
			return parser.TryParse (cssExpr, out result);
		}
	}
}

