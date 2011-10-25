/*
	Implementation.cs: Foreign API mapping
  
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
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Mono.Cecil;

namespace Cirrus.Tools.Cilc {
	
	public class Implementation {
		
		[Flags]
		public enum Option {
			// None: don't do anything; this implementation is a no-op
			None = 0,
			
			// Decompile: uses Cecil.Decompiler to decompile the managed code (the default)
			Decompile = 1,
			
			// NotImplemented: This causes an error if code compiled to target language uses this
			NotImplemented = 2,
			
			// InlineCode: Target language code is defined in the element
			//  Uses string.Format syntax for method args:
			//  "{0}" is this (if not static or constructor), "{1}" is arg.1, etc...
			//  "{...}" may only be used by itself, for var args (ex. Console.WriteLine), or as a wildcard
			InlineCode = 4,
			
			// Native: Don't emit anything, expect it to already be defined in target land
			Native = 8,
			// Additional options for Native:
				ChangeCase = 24,			// < converts "foo.CamelCaseIdentifier" to "foo.camelCaseIdentifier"
				AnonymousObject = 40,		// < for types with default constructor ONLY,
											//    JavaScript example: converts C#: "new Foo ()" to javascript: "{ }"
		}
		
		public string Name { get; set; }
		
		public Option Options { get; set; }
		public string InlineCode { get; set; }
				
		public string FormattedName  {
			get {
				if (formatted_name == null) {
					formatted_name = FormattedFullName;
					
					var i = formatted_name.LastIndexOf ('.');
					if (i > 0)
						formatted_name = formatted_name.Substring (i + 1);
					
					if (IsSet (Option.Native)) {
						//FIXME: compiler dependent?
						if (formatted_name.StartsWith ("get_") || formatted_name.StartsWith ("set_"))	
							formatted_name = formatted_name.Substring (4);
					}
					
					if (IsSet (Option.ChangeCase))	{
						var sb = new StringBuilder (formatted_name.Length);
						sb.Append (Char.ToLower (formatted_name [0]));
						sb.Append (formatted_name.Substring (1));
						formatted_name = sb.ToString();
					}
				}
				return formatted_name;
			}
			set {
				formatted_name = value;
			}
		}
		private string formatted_name;
		
		public string FormattedFullName {
			get {
				return Regex.Replace (Name, "[`\\<\\>]", "$").Replace ("/", ".");
			}
		}
		
		private XElement elem;
		
		public Implementation (string name) 
			: this ()
		{
			this.Name = name;	
		}
		
		protected Implementation ()
		{
			this.Options = Option.Decompile;
		}
		
		public bool IsSet (Option option)
		{
			return (Options & option) == option;
		}
		
		public Implementation ForMethodInvoke (MethodReference method)
		{
			if (elem != null) {
				XElement member = null;
				XElement child = null;
				
				//FIXME: Is this an optional convention for compilers?
				var isGetter = method.Name.StartsWith ("get_");
				var isSetter = method.Name.StartsWith ("set_");
				
				// .. or this?
				if (method.Name == ".ctor") {
					
					member = FindOverload (method, elem.Elements ("Constructor"));
					if (member != null)
						child = member.Elements ("Invoke").SingleOrDefault ();
					
				} else if (isGetter || isSetter) {
					
					member = elem.Elements ("Property").SingleOrDefault (p => p.Attribute ("Name").Value == method.Name.Substring (4));
					if (member != null && isGetter) {
						child = member.Elements ("Get").SingleOrDefault ();
						
					} else if (member != null && isSetter) {
						child = member.Elements ("Set").SingleOrDefault ();
					}
					
				} else {
					
					member = FindOverload (method, elem.Elements ("Method").Where (m => m.Attribute ("Name").Value == method.Name));
					if (member != null)
						child = member.Elements ("Invoke").SingleOrDefault ();
				}
				
				if (child != null)
					//FIXME: This could be better.
					return FromXElement (child, FromXElement (member, this));
				else if (member != null)
					return FromXElement (member, this);
			}
			
			// FIXME: This is kludgy
			var methodImpl = (Implementation)MemberwiseClone ();
			methodImpl.Name += "." + method.Name;
			methodImpl.formatted_name = null;
			return methodImpl;
		}
		
		private static XElement FindOverload (MethodReference needle, IEnumerable<XElement> haystack)
		{
			return (
				from m in haystack
				let str = ((string)m.Attribute ("Args")) ?? string.Empty
				let args = str.Split (new char [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select (arg => arg.Trim ())
				where str == "..." || args.SequenceEqual (needle.Parameters.Select (p => p.ParameterType.FullName))
				select m
			).SingleOrDefault ();
		}
		
		public static Implementation FromXElement (XElement member, Implementation parent)
		{
			var impl = new Implementation (
				((string)member.Attribute ("Name")) ?? parent.Name
			);
			impl.Options = Option.None;
			
			if (!member.HasElements && member.Value != null && member.Value.Trim () != "") {
				impl.Options |= Implementation.Option.InlineCode;
				impl.InlineCode = member.Value.Trim ();
				
			} else if (member.HasElements) {
				
				impl.elem = member;
			}
			
			var options = (string)member.Attribute ("Options");
			if (options != null) {
				foreach (var option in options.Split (new char [] { '|' }, StringSplitOptions.RemoveEmptyEntries))
					impl.Options |= (Implementation.Option)Enum.Parse (typeof (Implementation.Option), option, true);
			} else {
				impl.Options |= parent.Options;
			}
			
			return impl;
		}
		
		public static IEnumerable<Implementation> FromXDocument (XDocument doc)
		{			
			return from type in doc.Root.Elements ()
			       select FromXElement (type, new Implementation ());
		}
		
	}
}

