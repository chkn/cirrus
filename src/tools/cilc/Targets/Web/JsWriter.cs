using System;
using System.Linq;

using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Mono.Cecil;

using Cecil.Decompiler;
using Cecil.Decompiler.Ast;
using Cecil.Decompiler.Languages;

namespace Cirrus.Tools.Cilc.Targets.Web {
	public class JsWriter : ILanguageWriter {
		
		Scope globalScope = new Scope ();
		
		DecompilePipeline pipeline;
		StreamWriter writer;
		
		public static string CleanName (string name)
		{
			return Regex.Replace (name, "[`\\<\\>]", "$").Replace ("/", ".");
		}
		
		public JsWriter (DecompilePipeline pipeline, Stream stream)
		{
			this.pipeline = pipeline;
			this.writer = new StreamWriter (stream, Encoding.UTF8);
		}
		
		public void Write (TypeDefinition type)
		{
			if (WebType.ShouldProcess (type))
				Write (new WebType (pipeline, globalScope, type));
		}
		
		public void Write (WebType type)
		{
			bool first = true;
			var ns = type.Namespace;
			
			// namespace
			if (ns != null && !type.IsNested)
				writer.Write (ns == "Cirrus"? "{0}." : "Cirrus.$ns('{0}').", ns);
			
			// type name
			writer.Write (type.IsNested? "{0}:" : "{0}=", type.Name);
			
			if (!type.IsSimpleObject) {
			
				writer.Write ("Cirrus.$cls(\"{0}\",{1},{{", type.Name, type.BaseName);
				
				foreach (var field in type.Definition.Fields.Where (f => !f.IsStatic)) {
					if (!first)
						writer.Write (',');
					Write (field);
					first = false;
				}
				
				foreach (var method in type.Methods.Where (m => !m.Key.IsStatic)) {
					if (!first)
						writer.Write (',');
					Write (method.Value);
					first = false;
				}
				
				writer.Write ("},{");	
			} else {
				// IsSimpleObject
				writer.Write ("{");
			}
			
			first = true;
			foreach (var field in type.Definition.Fields.Where (f => f.IsStatic)) {
				if (!first)
					writer.Write (',');
				Write (field);
				first = false;
			}
			
			foreach (var method in type.Methods.Where (m => m.Key.IsStatic)) {
				if (!first)
					writer.Write (',');
				Write (method.Value);
				first = false;
			}
			
			foreach (var nested in type.NestedTypes) {
				if (!first)
					writer.Write (',');
				Write (nested.Value);
				first = false;
			}
			
			writer.Write (type.IsSimpleObject? "}" : "})");
			if (!type.IsNested)
				writer.Write (";");
			writer.Flush ();
		}
		
		public void Write (FieldDefinition field)
		{
			string initValue;
			
			if (field.HasConstant)
				initValue = field.Constant.ToString ();
			else if (field.FieldType.IsValueType)
				initValue = "0";
			else
				initValue = "null";
			
			writer.Write ("{0}:{1}", CleanName (field.Name), initValue);	
		}
		
		public void Write (MethodDefinition method)
		{
			if (WebMethod.ShouldProcess (method))
				Write (new WebMethod (new WebType (pipeline, globalScope, method.DeclaringType), pipeline, globalScope, method));
		}
		
		public void Write (WebMethod method)
		{
			writer.Write ("{0}:function({1}){{", method.Name,
                          string.Join (",", method.Definition.Parameters.Select (p => method.Scope.GetNameFor (p)).ToArray ()));
				foreach (var token in method.Body)
					writer.Write (token.JavaScript);
			writer.Write ("}");
			writer.Flush ();
		}
		
		public void Write (Expression expression)
		{
			throw new NotImplementedException ();
		}
		
		public void Write (Statement statement)
		{
			throw new NotImplementedException ();
		}
	}
}

