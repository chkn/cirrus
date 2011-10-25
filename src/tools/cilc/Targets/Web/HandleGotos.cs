using System;
using System.Collections.Generic;

using Cecil.Decompiler;
using Cecil.Decompiler.Steps;
using Cecil.Decompiler.Ast;

namespace Cirrus.Tools.Cilc.Targets.Web {
	
	public class HandleGotos : BaseCodeTransformer, IDecompilationStep  {
		
		public static readonly IDecompilationStep Instance = new HandleGotos ();
		
		bool hasGotos;
		
		public override ICollection<SwitchCase> Visit (SwitchCaseCollection cases)
		{
			foreach (var caseNode in cases)
				caseNode.Body = (BlockStatement) VisitBlockStatement (caseNode.Body);
			return cases;
		}
		
		public override ICodeNode VisitGotoStatement (GotoStatement node)
		{
			hasGotos = true;
			return base.VisitGotoStatement (node);
		}
		
		public BlockStatement Process (Cecil.Decompiler.DecompilationContext context, BlockStatement body)
		{
			hasGotos = false;
			
			var block = (BlockStatement) VisitBlockStatement (body);
			if (hasGotos) {
				block = new BlockStatement ();
				(new StatementDecompiler (BlockOptimization.Basic)).Process (context, block);
			} else
			    RemoveLastReturn.Instance.Process (context, block);
			return block;
		}
	}
}

