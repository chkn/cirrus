using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Mono.Cecil;
using Cecil.Decompiler.Ast;

namespace Cirrus.Tools.Cilc.Targets.Web {
	
	public static class JsImpl {
		
		private struct JsType {
			public TypeDefinition Type;
			public JsImplAttribute Attr;
		}
		
		private static Dictionary<string,JsType> store = new Dictionary<string,JsType> ();
		
		static JsImpl ()
		{
			//FIXME: kludgy ways to load these assemblies
			LoadJsImplModule (Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), "Cirrus.Platform.JavaScript.dll"));
			LoadJsImplModule (typeof (Future).Assembly.Location);
		}
		
		public static void LoadJsImplModule (string fileName)
		{
			var module = ModuleDefinition.ReadModule (fileName);
			
			foreach (var type in module.Types.Where (t => t.HasCustomAttributes)) {
				var attr = type.CustomAttributes.FirstOrDefault (ca => ca.AttributeType.FullName == typeof (JsImplAttribute).FullName);
				if (attr == null)
					continue;
				
				var jia = FromCustomAttribute (attr);
				store.Add (jia.For ?? type.FullName, new JsType { Type = type, Attr = jia});
			}	
		}
			
		public static JsImplAttribute For (TypeReference type)
		{
			var jt = JsTypeOf (type);
			return jt.Attr ?? JsImplAttribute.Default;
		}
		
		
		//FIXME: Deal with overloads
		public static JsImplAttribute For (MethodReference method)
		{
			var jt = JsTypeOf (method.DeclaringType);
			CustomAttribute jia = null;
			
			//FIXME: Compiler specific?
			if (method.Name.StartsWith ("get_") || method.Name.StartsWith ("set_")) {
				
				var prop = jt.Type.Properties.SingleOrDefault (p => p.Name == method.Name.Substring (4));
				if (prop != null)
					jia = prop.CustomAttributes.SingleOrDefault (ca => ca.AttributeType.FullName == typeof (JsImplAttribute).FullName);
				
			}
			
			if (jia == null) {
				
				//FIXME: handle overloads better?
				jia = (from m in jt.Type.Methods
				       where m.Name == method.Name &&
				             m.HasCustomAttributes &&
				             m.Parameters.Count == method.Parameters.Count
					   select m.CustomAttributes.SingleOrDefault (ca => ca.AttributeType.FullName == typeof (JsImplAttribute).FullName))
					.FirstOrDefault ();
				
			}
				
			if (jia == null)
				return jt.Attr ?? JsImplAttribute.Default;
			
			return FromCustomAttribute (jia);
		}
			    
		private static JsType JsTypeOf (TypeReference type)
		{
			JsType jt;
			if (type is GenericInstanceType)
				type = type.GetElementType ();
			
			if (!store.TryGetValue (type.FullName, out jt))
				return new JsType { Type = type.Resolve () };
			return jt;
		}
		
		private static JsImplAttribute FromCustomAttribute (CustomAttribute attr)
		{
			var jiaType = typeof (JsImplAttribute);
			var jia = (JsImplAttribute)Activator.CreateInstance (jiaType, attr.ConstructorArguments.Select (ca => ca.Value).ToArray ());
				
				foreach (var prop in attr.Properties)
					jiaType.GetProperty (prop.Name).SetValue (jia, prop.Argument.Value, null);
			
			return jia;
		}
		
	}
}

