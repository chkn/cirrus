using System;


using Cecil.Decompiler.Languages;
using Cecil.Decompiler.Steps;
using Cecil.Decompiler;

namespace Cirrus.Tools.Cilc.Targets.Web {
	public class DecompilePipeline : ILanguage {

		public DecompilationPipeline CreatePipeline ()
		{
			return new DecompilationPipeline (
				new StatementDecompiler (BlockOptimization.Detailed),
			    HandleGotos.Instance,
			    RebuildForStatements.Instance,
				RebuildForeachStatements.Instance,
			    //DeclareVariablesOnFirstAssignment.Instance,
				DeclareTopLevelVariables.Instance,
			    SelfAssignement.Instance,
			    TypeOfStep.Instance,
			    DelegateCreateStep.Instance,
			    DelegateInvokeStep.Instance,
				OperatorStep.Instance);
		}

		public string Name {
			get { return "Cirrus Web Target"; }
		}
		
		public ILanguageWriter GetWriter (IFormatter formatter)
		{
			throw new NotSupportedException ();
		}
	}
}

