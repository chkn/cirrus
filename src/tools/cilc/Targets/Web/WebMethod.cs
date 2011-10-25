using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Mono.Cecil;

using Cecil.Decompiler;
using Cecil.Decompiler.Ast;
using Cecil.Decompiler.Languages;

using Cirrus.Tools.Cilc;

namespace Cirrus.Tools.Cilc.Targets.Web {
	
	public class WebMethod : BaseCodeVisitor {
		
		public WebType ContainingType { get; private set; }
		public Scope Scope { get; private set; }
		public Implementation Options { get; private set; }
		
		public MethodDefinition Definition { get; private set; }
		public List<Token> Body { get; private set; }
		
		private bool inside_binary = false;
		
		private string fpVar = null;
		private Dictionary<string,int> fpTable = new Dictionary<string, int> ();
		private int maxFp = 1;
		
		private bool outputVar = false;
			
		public WebMethod (WebType type, DecompilePipeline pipeline, Scope parentScope, MethodDefinition def)
		{
			this.ContainingType = type;
			this.Scope = parentScope.Child ();
			this.Options = def.GetInvokeOptions ();
			
			this.Definition = def;
			this.Body = new List<Token> ();
			
			if (!def.HasBody)
				return;
			
			Visit (def.Body.Decompile (pipeline));
			if (fpVar != null) {
				Body.Add (Tokens.R_BRACK);
				Body.Add (Tokens.R_BRACK);
			}
		}
		
		public string Name {
			get {
				return Definition.IsConstructor? ContainingType.Name : Options.FormattedName;
			}
		}
		
		public static bool ShouldProcess (MethodDefinition method)
		{
			return method.GetInvokeOptions ().IsSet (Implementation.Option.Decompile);	
		}
		
		//*---------------------------*
		
		public override void VisitIfStatement (IfStatement node)
		{
			Body.Add (Tokens.IF);
			Visit (node.Condition);
			Body.Add (Tokens.R_PAREN);
			Body.Add (Tokens.L_BRACK);
			
			Visit (node.Then);
			
			Body.Add (Tokens.R_BRACK);
			if (node.Else == null)
				return;

			Body.Add (Tokens.ELSE);
			Body.Add (Tokens.L_BRACK);
			
			Visit (node.Else);
			
			Body.Add (Tokens.R_BRACK);
		}
		
		public override void VisitSwitchStatement (SwitchStatement node)
		{
			Body.Add (Tokens.SWITCH);
			Visit (node.Expression);
			Body.Add (Tokens.R_PAREN);
			Body.Add (Tokens.L_BRACK);

			Visit (node.Cases);
			
			Body.Add (Tokens.R_BRACK);
		}

		public override void VisitConditionCase (ConditionCase node)
		{
			Body.Add (Tokens.CASE);
			Visit (node.Condition);
			Body.Add (Tokens.COLON);

			Visit (node.Body);
		}

		public override void VisitDefaultCase (DefaultCase node)
		{
			Body.Add (Tokens.DEFAULT);
			Visit (node.Body);
		}
		
		public override void VisitForStatement (ForStatement node)
		{
			Body.Add (Tokens.FOR);
			Visit (node.Initializer);
			Visit (node.Condition);
			Body.Add (Tokens.SEMIC);
			Visit (((ExpressionStatement)node.Increment).Expression);
			Body.Add (Tokens.R_PAREN);
			Body.Add (Tokens.L_BRACK);
			Visit (node.Body);
			Body.Add (Tokens.R_BRACK);
		}

		public override void VisitForEachStatement (ForEachStatement node)
		{
			throw new NotImplementedException ();
		}

		
		public override void VisitWhileStatement (WhileStatement node)
		{
			Body.Add (Tokens.WHILE);
			Visit (node.Condition);
			Body.Add (Tokens.R_PAREN);
			Body.Add (Tokens.L_BRACK);
			Visit (node.Body);
			Body.Add (Tokens.R_BRACK);
		}

		public override void VisitDoWhileStatement (DoWhileStatement node)
		{
			Body.Add (Tokens.DO);
			Body.Add (Tokens.L_BRACK);
			Visit (node.Body);
			Body.Add (Tokens.R_BRACK);
			Body.Add (Tokens.WHILE);
			Visit (node.Condition);
			Body.Add (Tokens.R_PAREN);
			Body.Add (Tokens.SEMIC);
		}
		
		public override void VisitContinueStatement (ContinueStatement node)
		{
			Body.Add (Tokens.CONTINUE);
		}

		public override void VisitBreakStatement (BreakStatement node)
		{
			Body.Add (Tokens.BREAK);
			Body.Add (Tokens.SEMIC);
		}
		
		public override void VisitThrowStatement (ThrowStatement node)
		{
			Body.Add (Tokens.THROW);
			if (node.Expression != null) {
				
				Visit (node.Expression);
				
			} else {
				throw new NotImplementedException ("Rethrow exception");
			}
			
			Body.Add (Tokens.SEMIC);
		}
		
		public override void VisitTryStatement (TryStatement node)
		{
			// FIXME: Make this work with while gotos
			
			//Body.Add (Tokens.TRY);
			Visit (node.Try);
			//Body.Add (Tokens.R_BRACK);
			//Visit (node.CatchClauses);
			/*
			if (node.Finally != null) {
				Body.Add (Tokens.FINALLY);
				Visit (node.Finally);
				Body.Add (Tokens.R_BRACK);
			}
			*/
		}

		public override void VisitCatchClause (CatchClause node)
		{
			Body.Add (Tokens.CATCH);

			if (node.Variable != null)
				Visit (new VariableReferenceExpression (node.Variable.Variable));
			else
				Body.Add (Scope.GetName ());
			
			Body.Add (Tokens.R_PAREN);
			Body.Add (Tokens.L_BRACK);

			Visit (node.Body);
			
			Body.Add (Tokens.R_BRACK);
		}
		
		public override void VisitReturnStatement (ReturnStatement node)
		{
			Body.Add (Tokens.RETURN);

			if (node.Expression != null)
				Visit (node.Expression);
			
			Body.Add (Tokens.SEMIC);
		}
		
		public override void VisitExpressionStatement (ExpressionStatement node)
		{
			Visit (node.Expression);
			Body.Add (Tokens.SEMIC);
		}
		
		public override void VisitBlockStatement (BlockStatement node)
		{
			//Impl.Add (Tokens.L_BRACK);
			Visit (node.Statements);
			//Impl.Add (Tokens.R_BRACK);
		}
		
		public override void VisitBlockExpression (BlockExpression node)
		{
			VisitList (node.Expressions);
		}

		void VisitList (IList<Expression> list)
		{
			for (int i = 0; i < list.Count; i++) {
				if (i > 0)
					Body.Add (Tokens.COMMA);

				Visit (list [i]);
			}
		}
		
		public override void VisitTypeReferenceExpression (TypeReferenceExpression node)
		{
			Body.Add (ToToken (node.Type));
		}
		
		public override void VisitTypeOfExpression (TypeOfExpression node)
		{
			// FIXME:
			Body.Add ("typeof (" + ToToken (node.Type) + ")");
		}
		
		public override void VisitObjectCreationExpression (ObjectCreationExpression node)
		{
			TypeReference type;
			Implementation impl;
			
			if (node.Constructor != null) {
				type = node.Constructor.DeclaringType;
				impl = node.Constructor.GetInvokeOptions ();
				
			} else {
				type = node.Type;
				impl = type.GetImplementationOptions ();
				
			}
			
			if (impl.IsSet (Implementation.Option.NotImplemented))
				throw new Target.Error (Definition.Module.Name,
				                        string.Format ("use of class `{0}' is not supported for this target.", type.FullName));
			
			if (impl.IsSet (Implementation.Option.InlineCode)) {
				
				VisitInlineCode (impl.InlineCode, index => {
					if (index.HasValue)
						Visit (node.Arguments [index.Value]);
					else VisitList (node.Arguments);
				});
				
			} else if (impl.IsSet (Implementation.Option.AnonymousObject)) {
					
				Body.Add (Tokens.L_BRACK);
				Body.Add (Tokens.R_BRACK);
				
			} else {
			
				Body.Add (Tokens.NEW);
				Body.Add (ToToken (type));
				Body.Add (Tokens.L_PAREN);
				VisitList (node.Arguments);
				Body.Add (Tokens.R_PAREN);
			}
		}
		
		public override void VisitDelegateCreationExpression (DelegateCreationExpression node)
		{
			if (node.Target != null) {
				Visit (node.Target);
				Body.Add (Tokens.DOT);
			}

			if (!node.Method.HasThis) {
				Body.Add (ToToken (node.Method.DeclaringType));
				Body.Add (Tokens.DOT);
			}
			
			Body.Add (node.Method.GetInvokeOptions ().FormattedName);
			
			if (node.Target != null) {
				Body.Add (Tokens.BIND);
				Visit (node.Target);
				Body.Add (Tokens.R_PAREN);
			}
		}
		
		public override void VisitDelegateInvocationExpression (DelegateInvocationExpression node)
		{
			Visit (node.Target);
			Body.Add (Tokens.L_PAREN);
			Visit (node.Arguments);
			Body.Add (Tokens.R_PAREN);
		}
		
		public Token ToToken (TypeReference type)
		{
			if (type.IsGenericInstance)
				return ToToken (type.GetElementType());
			return type.GetImplementationOptions ().FormattedFullName;
		}
		
		public override void VisitArrayCreationExpression (ArrayCreationExpression node)
		{
			Body.Add (Tokens.ARRAY);
		}

		public override void VisitArrayIndexerExpression (ArrayIndexerExpression node)
		{
			Visit (node.Target);
			Body.Add (Tokens.L_BRACE);
			Visit (node.Indices);
			Body.Add (Tokens.R_BRACE);
		}
		
		public override void VisitVariableDeclarationExpression (VariableDeclarationExpression node)
		{
			if (!outputVar) {
				Body.Add (Tokens.VAR);
			} else {
				Body.RemoveAt (Body.Count - 1);
				Body.Add (Tokens.COMMA);
			}
			
			Body.Add (Scope.GetNameFor (node.Variable));
			outputVar = true;
		}
		
		public override void VisitVariableReferenceExpression (VariableReferenceExpression node)
		{
			Body.Add (Scope.GetNameFor (node.Variable));
		}
		
		public override void VisitArgumentReferenceExpression (ArgumentReferenceExpression node)
		{
			Body.Add (Scope.GetNameFor (node.Parameter));
		}

		public override void VisitAssignExpression (AssignExpression node)
		{
			Visit (node.Target);
			Body.Add (Tokens.EQUALS);
			Visit (node.Expression);
		}
		
		public override void VisitMethodReferenceExpression (MethodReferenceExpression node)
		{
			var impl = node.Method.GetInvokeOptions ();
			
			if (node.Target != null) {
				Visit (node.Target);
				Body.Add (Tokens.DOT);
			}

			if (!node.Method.HasThis) {
				Body.Add (ToToken (node.Method.DeclaringType));
				Body.Add (Tokens.DOT);
			}
			
			//FIXME: better way to determine ctor
			var name = node.Method.Name == ".ctor"?
				node.Method.DeclaringType.GetImplementationOptions ().FormattedName : 
				impl.FormattedName;
			
			Body.Add (name);
		}
		
		public override void VisitMethodInvocationExpression (MethodInvocationExpression node)
		{	
			var methodRef = (MethodReferenceExpression)node.Method;
			var jia = methodRef.Method.GetInvokeOptions ();
			
			if (jia.IsSet (Implementation.Option.NotImplemented))
				throw new Target.Error (Definition.Module.Name,
				                        string.Format ("call to method `{0}' not supported for this target.", methodRef.Method.FullName));
			
			//FIXME: Compiler specific... not so good anyway
			var isGetter = methodRef.Method.Name.StartsWith ("get_");
			var isSetter = methodRef.Method.Name.StartsWith ("set_");
			
			if (jia.IsSet (Implementation.Option.InlineCode)) {
				
				VisitInlineCode (jia.InlineCode, index => {
					if (!index.HasValue)
						VisitList (node.Arguments);
					else if (index == 0)
						Visit (methodRef.Method.HasThis? methodRef.Target : node.Arguments [0]);
					else
						Visit (node.Arguments [methodRef.Method.HasThis? index.Value - 1 : index.Value]);	
				});
				
			} else {
				
				VisitMethodReferenceExpression (methodRef);
				
				if (isSetter && jia.IsSet (Implementation.Option.Native)) {
						Body.Add (Tokens.EQUALS);
						Visit (node.Arguments [0]);
					
				} else if (!isGetter || !jia.IsSet (Implementation.Option.Native)) {
					Body.Add (Tokens.L_PAREN);
					VisitList (node.Arguments);
					Body.Add (Tokens.R_PAREN);
				}
			}
		}
		
		public void VisitInlineCode (string code, Action<int?> visitPart)
		{
			Match match;
				
			while (((match = Regex.Match (code, @"(?<!\{)\{(\d+|\.{3})\}")) != null) && match.Success) {
				if (match.Index > 0)
					Body.Add (code.Substring (0, match.Index).Replace ("{{", "{").Replace ("}}", "}"));
				
				var wildcard = match.Groups [1].Value;
				if (wildcard == "...")
					visitPart (null);
				else 
					visitPart (int.Parse (wildcard));
				
				code = code.Substring (match.Index + match.Length);
			}
			
			if (code != "")
				Body.Add (code.Replace ("{{", "{").Replace ("}}", "}"));
		}
		
		public override void VisitThisReferenceExpression (ThisReferenceExpression node)
		{
			Body.Add (Tokens.THIS);
		}
		
		public override void VisitBaseReferenceExpression (BaseReferenceExpression node)
		{
			//Impl.Add (Tokens.BASE);
			throw new NotImplementedException ();
		}
		
		public override void VisitFieldReferenceExpression (FieldReferenceExpression node)
		{
			if (node.Target != null)
				Visit (node.Target);
			else
				Body.Add (ToToken (node.Field.DeclaringType));
			
			Body.Add (Tokens.DOT);
			Body.Add (JsWriter.CleanName (node.Field.Name));
		}
		
		public override void VisitConditionExpression (ConditionExpression node)
		{
			Body.Add (Tokens.L_PAREN);
			Visit (node.Condition);
			Body.Add (Tokens.QUESTION);
			Visit (node.Then);
			Body.Add (Tokens.COLON);
			Visit (node.Else);
			Body.Add (Tokens.R_PAREN);
		}
		
		public override void VisitNullCoalesceExpression (NullCoalesceExpression node)
		{
			Visit (node.Condition);
			Body.Add (Tokens.OR);
			Visit (node.Expression);
		}
		
		public override void VisitGotoStatement (GotoStatement node)
		{
			if (fpVar == null)
				ConvertToWhileSwitch ();
			
			Body.Add (fpVar);
			Body.Add (Tokens.EQUALS);
			Body.Add (GetFpForLabel (node.Label).ToString ());
			Body.Add (Tokens.SEMIC);
			Body.Add (Tokens.BREAK);
			Body.Add ("cxbr");
			Body.Add (Tokens.SEMIC);
		}
		
		public override void VisitLabeledStatement (LabeledStatement node)
		{
			if (fpVar == null)
				ConvertToWhileSwitch ();
			
			Body.Add (Tokens.CASE);
			Body.Add (GetFpForLabel (node.Label).ToString ());
			Body.Add (Tokens.COLON);
		}
		
		
		private void ConvertToWhileSwitch ()
		{
			fpVar = Scope.GetName ();
			var prefix = new Token [] {
				Tokens.VAR,fpVar,Tokens.EQUALS,"0",Tokens.SEMIC,
				Tokens.WHILE,Tokens.TRUE,Tokens.R_PAREN,Tokens.L_BRACK,
				"cxbr",Tokens.COLON,Tokens.SWITCH,fpVar,Tokens.R_PAREN,Tokens.L_BRACK,
				Tokens.CASE,"0",Tokens.COLON
			};
			Body.InsertRange (0, prefix);
		}
		private int GetFpForLabel (string label)
		{
			int fp;
			if (!fpTable.TryGetValue (label, out fp)) {
				fp = maxFp++;
				fpTable.Add (label, fp);
			}
			return fp;
		}
		
		//FIXME: Escape strings
		public override void VisitLiteralExpression (LiteralExpression node)
		{
			var value = node.Value;
			if (value == null) {
				Body.Add (Tokens.NULL);
				return;
			}

			switch (Type.GetTypeCode (value.GetType ())) {
			case TypeCode.Boolean:
				Body.Add ((bool) value ? Tokens.TRUE : Tokens.FALSE);
				return;
			case TypeCode.String:
				Body.Add (Tokens.QUOTE);
				Body.Add (value.ToString ());
				Body.Add (Tokens.QUOTE);
				return;
			// complete
			default:
				Body.Add (value.ToString ());
				return;
			}
		}
		
		public override void VisitUnaryExpression (UnaryExpression node)
		{
			bool is_post_op = IsPostUnaryOperator (node.Operator);

			if (!is_post_op)
				Body.Add (ToToken (node.Operator));
			
			Visit (node.Operand);

			if (is_post_op)
				Body.Add (ToToken (node.Operator));
		}
		
		static bool IsPostUnaryOperator (UnaryOperator op)
		{
			switch (op) {
			case UnaryOperator.PostIncrement:
			case UnaryOperator.PostDecrement:
				return true;
			default:
				return false;
			}
		}
		
		public override void VisitBinaryExpression (BinaryExpression node)
		{
			var was_inside = inside_binary;
			inside_binary = true;

			if (was_inside)
				Body.Add (Tokens.L_PAREN);
			Visit (node.Left);
			Body.Add (ToToken (node.Operator));
			Visit (node.Right);
			if (was_inside)
				Body.Add (Tokens.R_PAREN);

			inside_binary = was_inside;
		}
		
		static Token ToToken (BinaryOperator op)
		{
			switch (op) {
			case BinaryOperator.Add: return Tokens.PLUS;
			case BinaryOperator.BitwiseAnd: return Tokens.AMP;
			case BinaryOperator.BitwiseOr: return Tokens.PIPE;
			case BinaryOperator.BitwiseXor: return Tokens.XOR;
			case BinaryOperator.Divide: return Tokens.DIV;
			case BinaryOperator.GreaterThan: return Tokens.GT;
			case BinaryOperator.GreaterThanOrEqual: return Tokens.GT_EQ;
			case BinaryOperator.LeftShift: return "<<";
			case BinaryOperator.LessThan: return Tokens.LT;
			case BinaryOperator.LessThanOrEqual: return Tokens.LT_EQ;
			case BinaryOperator.LogicalAnd: return Tokens.AND;
			case BinaryOperator.LogicalOr: return Tokens.OR;
			case BinaryOperator.Modulo: return Tokens.MODULO;
			case BinaryOperator.Multiply: return Tokens.MULTIPLY;
			case BinaryOperator.RightShift: return ">>";
			case BinaryOperator.Subtract: return Tokens.MINUS;
			case BinaryOperator.ValueEquality: return Tokens.EQUALITY;
			case BinaryOperator.ValueInequality: return Tokens.INEQUALITY;
			default: throw new ArgumentException ();
			}
		}
		
		static Token ToToken (UnaryOperator op)
		{
			switch (op) {
			case UnaryOperator.BitwiseNot:
				return Tokens.TILDE;
			case UnaryOperator.LogicalNot:
				return Tokens.NOT;
			case UnaryOperator.Negate:
				return Tokens.MINUS;
			case UnaryOperator.PostDecrement:
			case UnaryOperator.PreDecrement:
				return Tokens.DECREMENT;
			case UnaryOperator.PostIncrement:
			case UnaryOperator.PreIncrement:
				return Tokens.INCREMENT;
			default: throw new ArgumentException ();
			}
		}
	}

}

