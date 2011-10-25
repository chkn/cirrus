using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Mono.Cecil;
using Cirrus.Tools.Cilc.Util;

namespace Cirrus.Tools.Cilc.Targets.Web {
	public class WebType {
		
		public Scope Scope { get; private set; }
		public TypeDefinition Definition { get; protected set; }
		public Implementation Options { get; protected set; }
		
		public OrderedDictionary<TypeDefinition,WebType> NestedTypes { get; private set; }
		public OrderedDictionary<MethodDefinition,WebMethod> Methods { get; private set; }
		
		internal WebType (DecompilePipeline pipeline, Scope parentScope, TypeDefinition def)
		{
			this.Scope = parentScope.Child ();
			this.Definition = def;
			this.Options = def.GetImplementationOptions ();
			
			this.NestedTypes = new OrderedDictionary<TypeDefinition, WebType> ();
			this.Methods = new OrderedDictionary<MethodDefinition,WebMethod> ();
			
			foreach (var type in def.NestedTypes) {
				if (ShouldProcess (type))
					NestedTypes.Add (type, new WebType (pipeline, Scope, type));
			}
			
			foreach (var method in def.Methods) {
				if (WebMethod.ShouldProcess (method))
					Methods.Add (method, new WebMethod (this, pipeline, Scope, method));
			}
		}
		
		public bool IsNested {
			get { return Definition.IsNested; }	
		}
		
		public string FullName {
			get {
				return Options.FormattedFullName;
			}
		}
		
		public string Namespace {
			get {
				var fn = FullName;
				var i = fn.LastIndexOf ('.');
				if (i > 0)
					return fn.Substring (0, i);
				return null;
			}
		}
		
		public string Name {
			get {
				var fn = FullName;
				var i = fn.LastIndexOf ('.');
				if (i > 0)
					return fn.Substring (i + 1);
				return null;
			}
		}
		
		public string BaseName {
			get {
				if (Definition.BaseType != null && Definition.BaseType.FullName != "System.Object")
					return Definition.BaseType.GetImplementationOptions ().FormattedName;
				return "Object";
			}
		}
		
		public bool IsSimpleObject {
			get {
				return Definition.IsEnum ||
						Definition.Methods.All (m => m.IsStatic);
				
			}
		}
		
		public static bool ShouldProcess (TypeDefinition type)
		{
			return type.GetImplementationOptions ().IsSet (Implementation.Option.Decompile) &&
				!type.IsInterface &&
				type.BaseType != null &&
				type.BaseType.FullName != "System.Attribute" &&
				type.BaseType.FullName != "System.Exception";	
		}
	}
}

