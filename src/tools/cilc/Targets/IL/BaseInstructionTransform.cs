using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Cirrus;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Cecil.Decompiler.Cil;

using Cirrus.Tools.Cilc.Util;

namespace Cirrus.Tools.Cilc.Targets.IL {

	public abstract class BaseInstructionTransform {
		
		public static Func<Instruction,bool> IsBarrier = i => false;
		
		// maps (original Instruction) -> (List of replacement Instructions)
		public OrderedDictionary<Instruction,IList<Instruction>> InstructionMap { get; protected set; }
		
		protected ControlFlowGraph cfg;
		protected InstructionBlock block;
		
		public BaseInstructionTransform ()
		{
			InstructionMap = new OrderedDictionary<Instruction, IList<Instruction>> ();
		}

		protected virtual void Visit (MethodDefinition method)
		{
			IEnumerable<Instruction> transformed = null;
			
			this.cfg = ControlFlowGraph.Create (method);
			
			foreach (var b in cfg.Blocks) {
				block = b;
				
				foreach (var instruction in block) {
					switch (instruction.OpCode.Code) {
					case Code.Nop:
						transformed = this.OnNop (instruction);
						break;
					case Code.Break:
						transformed = this.OnBreak (instruction);
						break;
					case Code.Ldarg_0:
						transformed = this.OnLdarg_0 (instruction);
						break;
					case Code.Ldarg_1:
						transformed = this.OnLdarg_1 (instruction);
						break;
					case Code.Ldarg_2:
						transformed = this.OnLdarg_2 (instruction);
						break;
					case Code.Ldarg_3:
						transformed = this.OnLdarg_3 (instruction);
						break;
					case Code.Ldloc_0:
						transformed = this.OnLdloc_0 (instruction);
						break;
					case Code.Ldloc_1:
						transformed = this.OnLdloc_1 (instruction);
						break;
					case Code.Ldloc_2:
						transformed = this.OnLdloc_2 (instruction);
						break;
					case Code.Ldloc_3:
						transformed = this.OnLdloc_3 (instruction);
						break;
					case Code.Stloc_0:
						transformed = this.OnStloc_0 (instruction);
						break;
					case Code.Stloc_1:
						transformed = this.OnStloc_1 (instruction);
						break;
					case Code.Stloc_2:
						transformed = this.OnStloc_2 (instruction);
						break;
					case Code.Stloc_3:
						transformed = this.OnStloc_3 (instruction);
						break;
					case Code.Ldarg:
					case Code.Ldarg_S:
						transformed = this.OnLdarg (instruction);
						break;
					case Code.Ldarga:
					case Code.Ldarga_S:
						transformed = this.OnLdarga (instruction);
						break;
					case Code.Starg:
					case Code.Starg_S:
						transformed = this.OnStarg (instruction);
						break;
					case Code.Ldloc:
					case Code.Ldloc_S:
						transformed = this.OnLdloc (instruction);
						break;
					case Code.Ldloca:
					case Code.Ldloca_S:
						transformed = this.OnLdloca (instruction);
						break;
					case Code.Stloc:
					case Code.Stloc_S:
						transformed = this.OnStloc (instruction);
						break;
					case Code.Ldnull:
						transformed = this.OnLdnull (instruction);
						break;
					case Code.Ldc_I4_M1:
						transformed = this.OnLdc_I4_M1 (instruction);
						break;
					case Code.Ldc_I4_0:
						transformed = this.OnLdc_I4_0 (instruction);
						break;
					case Code.Ldc_I4_1:
						transformed = this.OnLdc_I4_1 (instruction);
						break;
					case Code.Ldc_I4_2:
						transformed = this.OnLdc_I4_2 (instruction);
						break;
					case Code.Ldc_I4_3:
						transformed = this.OnLdc_I4_3 (instruction);
						break;
					case Code.Ldc_I4_4:
						transformed = this.OnLdc_I4_4 (instruction);
						break;
					case Code.Ldc_I4_5:
						transformed = this.OnLdc_I4_5 (instruction);
						break;
					case Code.Ldc_I4_6:
						transformed = this.OnLdc_I4_6 (instruction);
						break;
					case Code.Ldc_I4_7:
						transformed = this.OnLdc_I4_7 (instruction);
						break;
					case Code.Ldc_I4_8:
						transformed = this.OnLdc_I4_8 (instruction);
						break;
					case Code.Ldc_I4:
					case Code.Ldc_I4_S:
						transformed = this.OnLdc_I4 (instruction);
						break;
					case Code.Ldc_I8:
						transformed = this.OnLdc_I8 (instruction);
						break;
					case Code.Ldc_R4:
						transformed = this.OnLdc_R4 (instruction);
						break;
					case Code.Ldc_R8:
						transformed = this.OnLdc_R8 (instruction);
						break;
					case Code.Dup:
						transformed = this.OnDup (instruction);
						break;
					case Code.Pop:
						transformed = this.OnPop (instruction);
						break;
					case Code.Jmp:
						transformed = this.OnJmp (instruction);
						break;
					case Code.Call:
						transformed = this.OnCall (instruction);
						break;
					case Code.Calli:
						transformed = this.OnCalli (instruction);
						break;
					case Code.Ret:
						transformed = this.OnRet (instruction);
						break;
					case Code.Br:
					case Code.Br_S:
						transformed = this.OnBr (instruction);
						break;
					case Code.Brfalse:
					case Code.Brfalse_S:
						transformed = this.OnBrfalse (instruction);
						break;
					case Code.Brtrue:
					case Code.Brtrue_S:
						transformed = this.OnBrtrue (instruction);
						break;
					case Code.Beq:
					case Code.Beq_S:
						transformed = this.OnBeq (instruction);
						break;
					case Code.Bge:
					case Code.Bge_S:
						transformed = this.OnBge (instruction);
						break;
					case Code.Bgt:
					case Code.Bgt_S:
						transformed = this.OnBgt (instruction);
						break;
					case Code.Ble:
					case Code.Ble_S:
						transformed = this.OnBle (instruction);
						break;
					case Code.Blt:
					case Code.Blt_S:
						transformed = this.OnBlt (instruction);
						break;
					case Code.Bne_Un:
					case Code.Bne_Un_S:
						transformed = this.OnBne_Un (instruction);
						break;
					case Code.Bge_Un:
					case Code.Bge_Un_S:
						transformed = this.OnBge_Un (instruction);
						break;
					case Code.Bgt_Un:
					case Code.Bgt_Un_S:
						transformed = this.OnBgt_Un (instruction);
						break;
					case Code.Ble_Un:
					case Code.Ble_Un_S:
						transformed = this.OnBle_Un (instruction);
						break;
					case Code.Blt_Un:
					case Code.Blt_Un_S:
						transformed = this.OnBlt_Un (instruction);
						break;
					case Code.Switch:
						transformed = this.OnSwitch (instruction);
						break;
					case Code.Ldind_I1:
						transformed = this.OnLdind_I1 (instruction);
						break;
					case Code.Ldind_U1:
						transformed = this.OnLdind_U1 (instruction);
						break;
					case Code.Ldind_I2:
						transformed = this.OnLdind_I2 (instruction);
						break;
					case Code.Ldind_U2:
						transformed = this.OnLdind_U2 (instruction);
						break;
					case Code.Ldind_I4:
						transformed = this.OnLdind_I4 (instruction);
						break;
					case Code.Ldind_U4:
						transformed = this.OnLdind_U4 (instruction);
						break;
					case Code.Ldind_I8:
						transformed = this.OnLdind_I8 (instruction);
						break;
					case Code.Ldind_I:
						transformed = this.OnLdind_I (instruction);
						break;
					case Code.Ldind_R4:
						transformed = this.OnLdind_R4 (instruction);
						break;
					case Code.Ldind_R8:
						transformed = this.OnLdind_R8 (instruction);
						break;
					case Code.Ldind_Ref:
						transformed = this.OnLdind_Ref (instruction);
						break;
					case Code.Stind_Ref:
						transformed = this.OnStind_Ref (instruction);
						break;
					case Code.Stind_I1:
						transformed = this.OnStind_I1 (instruction);
						break;
					case Code.Stind_I2:
						transformed = this.OnStind_I2 (instruction);
						break;
					case Code.Stind_I4:
						transformed = this.OnStind_I4 (instruction);
						break;
					case Code.Stind_I8:
						transformed = this.OnStind_I8 (instruction);
						break;
					case Code.Stind_R4:
						transformed = this.OnStind_R4 (instruction);
						break;
					case Code.Stind_R8:
						transformed = this.OnStind_R8 (instruction);
						break;
					case Code.Add:
						transformed = this.OnAdd (instruction);
						break;
					case Code.Sub:
						transformed = this.OnSub (instruction);
						break;
					case Code.Mul:
						transformed = this.OnMul (instruction);
						break;
					case Code.Div:
						transformed = this.OnDiv (instruction);
						break;
					case Code.Div_Un:
						transformed = this.OnDiv_Un (instruction);
						break;
					case Code.Rem:
						transformed = this.OnRem (instruction);
						break;
					case Code.Rem_Un:
						transformed = this.OnRem_Un (instruction);
						break;
					case Code.And:
						transformed = this.OnAnd (instruction);
						break;
					case Code.Or:
						transformed = this.OnOr (instruction);
						break;
					case Code.Xor:
						transformed = this.OnXor (instruction);
						break;
					case Code.Shl:
						transformed = this.OnShl (instruction);
						break;
					case Code.Shr:
						transformed = this.OnShr (instruction);
						break;
					case Code.Shr_Un:
						transformed = this.OnShr_Un (instruction);
						break;
					case Code.Neg:
						transformed = this.OnNeg (instruction);
						break;
					case Code.Not:
						transformed = this.OnNot (instruction);
						break;
					case Code.Conv_I1:
						transformed = this.OnConv_I1 (instruction);
						break;
					case Code.Conv_I2:
						transformed = this.OnConv_I2 (instruction);
						break;
					case Code.Conv_I4:
						transformed = this.OnConv_I4 (instruction);
						break;
					case Code.Conv_I8:
						transformed = this.OnConv_I8 (instruction);
						break;
					case Code.Conv_R4:
						transformed = this.OnConv_R4 (instruction);
						break;
					case Code.Conv_R8:
						transformed = this.OnConv_R8 (instruction);
						break;
					case Code.Conv_U4:
						transformed = this.OnConv_U4 (instruction);
						break;
					case Code.Conv_U8:
						transformed = this.OnConv_U8 (instruction);
						break;
					case Code.Callvirt:
						transformed = this.OnCallvirt (instruction);
						break;
					case Code.Cpobj:
						transformed = this.OnCpobj (instruction);
						break;
					case Code.Ldobj:
						transformed = this.OnLdobj (instruction);
						break;
					case Code.Ldstr:
						transformed = this.OnLdstr (instruction);
						break;
					case Code.Newobj:
						transformed = this.OnNewobj (instruction);
						break;
					case Code.Castclass:
						transformed = this.OnCastclass (instruction);
						break;
					case Code.Isinst:
						transformed = this.OnIsinst (instruction);
						break;
					case Code.Conv_R_Un:
						transformed = this.OnConv_R_Un (instruction);
						break;
					case Code.Unbox:
						transformed = this.OnUnbox (instruction);
						break;
					case Code.Throw:
						transformed = this.OnThrow (instruction);
						break;
					case Code.Ldfld:
						transformed = this.OnLdfld (instruction);
						break;
					case Code.Ldflda:
						transformed = this.OnLdflda (instruction);
						break;
					case Code.Stfld:
						transformed = this.OnStfld (instruction);
						break;
					case Code.Ldsfld:
						transformed = this.OnLdsfld (instruction);
						break;
					case Code.Ldsflda:
						transformed = this.OnLdsflda (instruction);
						break;
					case Code.Stsfld:
						transformed = this.OnStsfld (instruction);
						break;
					case Code.Stobj:
						transformed = this.OnStobj (instruction);
						break;
					case Code.Conv_Ovf_I1_Un:
						transformed = this.OnConv_Ovf_I1_Un (instruction);
						break;
					case Code.Conv_Ovf_I2_Un:
						transformed = this.OnConv_Ovf_I2_Un (instruction);
						break;
					case Code.Conv_Ovf_I4_Un:
						transformed = this.OnConv_Ovf_I4_Un (instruction);
						break;
					case Code.Conv_Ovf_I8_Un:
						transformed = this.OnConv_Ovf_I8_Un (instruction);
						break;
					case Code.Conv_Ovf_U1_Un:
						transformed = this.OnConv_Ovf_U1_Un (instruction);
						break;
					case Code.Conv_Ovf_U2_Un:
						transformed = this.OnConv_Ovf_U2_Un (instruction);
						break;
					case Code.Conv_Ovf_U4_Un:
						transformed = this.OnConv_Ovf_U4_Un (instruction);
						break;
					case Code.Conv_Ovf_U8_Un:
						transformed = this.OnConv_Ovf_U8_Un (instruction);
						break;
					case Code.Conv_Ovf_I_Un:
						transformed = this.OnConv_Ovf_I_Un (instruction);
						break;
					case Code.Conv_Ovf_U_Un:
						transformed = this.OnConv_Ovf_U_Un (instruction);
						break;
					case Code.Box:
						transformed = this.OnBox (instruction);
						break;
					case Code.Newarr:
						transformed = this.OnNewarr (instruction);
						break;
					case Code.Ldlen:
						transformed = this.OnLdlen (instruction);
						break;
					case Code.Ldelema:
						transformed = this.OnLdelema (instruction);
						break;
					case Code.Ldelem_I1:
						transformed = this.OnLdelem_I1 (instruction);
						break;
					case Code.Ldelem_U1:
						transformed = this.OnLdelem_U1 (instruction);
						break;
					case Code.Ldelem_I2:
						transformed = this.OnLdelem_I2 (instruction);
						break;
					case Code.Ldelem_U2:
						transformed = this.OnLdelem_U2 (instruction);
						break;
					case Code.Ldelem_I4:
						transformed = this.OnLdelem_I4 (instruction);
						break;
					case Code.Ldelem_U4:
						transformed = this.OnLdelem_U4 (instruction);
						break;
					case Code.Ldelem_I8:
						transformed = this.OnLdelem_I8 (instruction);
						break;
					case Code.Ldelem_I:
						transformed = this.OnLdelem_I (instruction);
						break;
					case Code.Ldelem_R4:
						transformed = this.OnLdelem_R4 (instruction);
						break;
					case Code.Ldelem_R8:
						transformed = this.OnLdelem_R8 (instruction);
						break;
					case Code.Ldelem_Ref:
						transformed = this.OnLdelem_Ref (instruction);
						break;
					case Code.Stelem_I:
						transformed = this.OnStelem_I (instruction);
						break;
					case Code.Stelem_I1:
						transformed = this.OnStelem_I1 (instruction);
						break;
					case Code.Stelem_I2:
						transformed = this.OnStelem_I2 (instruction);
						break;
					case Code.Stelem_I4:
						transformed = this.OnStelem_I4 (instruction);
						break;
					case Code.Stelem_I8:
						transformed = this.OnStelem_I8 (instruction);
						break;
					case Code.Stelem_R4:
						transformed = this.OnStelem_R4 (instruction);
						break;
					case Code.Stelem_R8:
						transformed = this.OnStelem_R8 (instruction);
						break;
					case Code.Stelem_Ref:
						transformed = this.OnStelem_Ref (instruction);
						break;
					case Code.Ldelem_Any:
						transformed = this.OnLdelem_Any (instruction);
						break;
					case Code.Stelem_Any:
						transformed = this.OnStelem_Any (instruction);
						break;
					case Code.Unbox_Any:
						transformed = this.OnUnbox_Any (instruction);
						break;
					case Code.Conv_Ovf_I1:
						transformed = this.OnConv_Ovf_I1 (instruction);
						break;
					case Code.Conv_Ovf_U1:
						transformed = this.OnConv_Ovf_U1 (instruction);
						break;
					case Code.Conv_Ovf_I2:
						transformed = this.OnConv_Ovf_I2 (instruction);
						break;
					case Code.Conv_Ovf_U2:
						transformed = this.OnConv_Ovf_U2 (instruction);
						break;
					case Code.Conv_Ovf_I4:
						transformed = this.OnConv_Ovf_I4 (instruction);
						break;
					case Code.Conv_Ovf_U4:
						transformed = this.OnConv_Ovf_U4 (instruction);
						break;
					case Code.Conv_Ovf_I8:
						transformed = this.OnConv_Ovf_I8 (instruction);
						break;
					case Code.Conv_Ovf_U8:
						transformed = this.OnConv_Ovf_U8 (instruction);
						break;
					case Code.Refanyval:
						transformed = this.OnRefanyval (instruction);
						break;
					case Code.Ckfinite:
						transformed = this.OnCkfinite (instruction);
						break;
					case Code.Mkrefany:
						transformed = this.OnMkrefany (instruction);
						break;
					case Code.Ldtoken:
						transformed = this.OnLdtoken (instruction);
						break;
					case Code.Conv_U2:
						transformed = this.OnConv_U2 (instruction);
						break;
					case Code.Conv_U1:
						transformed = this.OnConv_U1 (instruction);
						break;
					case Code.Conv_I:
						transformed = this.OnConv_I (instruction);
						break;
					case Code.Conv_Ovf_I:
						transformed = this.OnConv_Ovf_I (instruction);
						break;
					case Code.Conv_Ovf_U:
						transformed = this.OnConv_Ovf_U (instruction);
						break;
					case Code.Add_Ovf:
						transformed = this.OnAdd_Ovf (instruction);
						break;
					case Code.Add_Ovf_Un:
						transformed = this.OnAdd_Ovf_Un (instruction);
						break;
					case Code.Mul_Ovf:
						transformed = this.OnMul_Ovf (instruction);
						break;
					case Code.Mul_Ovf_Un:
						transformed = this.OnMul_Ovf_Un (instruction);
						break;
					case Code.Sub_Ovf:
						transformed = this.OnSub_Ovf (instruction);
						break;
					case Code.Sub_Ovf_Un:
						transformed = this.OnSub_Ovf_Un (instruction);
						break;
					case Code.Endfinally:
						transformed = this.OnEndfinally (instruction);
						break;
					case Code.Leave:
					case Code.Leave_S:
						transformed = this.OnLeave (instruction);
						break;
					case Code.Stind_I:
						transformed = this.OnStind_I (instruction);
						break;
					case Code.Conv_U:
						transformed = this.OnConv_U (instruction);
						break;
					case Code.Arglist:
						transformed = this.OnArglist (instruction);
						break;
					case Code.Ceq:
						transformed = this.OnCeq (instruction);
						break;
					case Code.Cgt:
						transformed = this.OnCgt (instruction);
						break;
					case Code.Cgt_Un:
						transformed = this.OnCgt_Un (instruction);
						break;
					case Code.Clt:
						transformed = this.OnClt (instruction);
						break;
					case Code.Clt_Un:
						transformed = this.OnClt_Un (instruction);
						break;
					case Code.Ldftn:
						transformed = this.OnLdftn (instruction);
						break;
					case Code.Ldvirtftn:
						transformed = this.OnLdvirtftn (instruction);
						break;
					case Code.Localloc:
						transformed = this.OnLocalloc (instruction);
						break;
					case Code.Endfilter:
						transformed = this.OnEndfilter (instruction);
						break;
					case Code.Unaligned:
						transformed = this.OnUnaligned (instruction);
						break;
					case Code.Volatile:
						transformed = this.OnVolatile (instruction);
						break;
					case Code.Tail:
						transformed = this.OnTail (instruction);
						break;
					case Code.Initobj:
						transformed = this.OnInitobj (instruction);
						break;
					case Code.Cpblk:
						transformed = this.OnCpblk (instruction);
						break;
					case Code.Initblk:
						transformed = this.OnInitblk (instruction);
						break;
					case Code.Rethrow:
						transformed = this.OnRethrow (instruction);
						break;
					case Code.Sizeof:
						transformed = this.OnSizeof (instruction);
						break;
					case Code.Refanytype:
						transformed = this.OnRefanytype (instruction);
						break;
					case Code.Constrained:
						transformed = this.OnConstrained (instruction);
						break;
					default:
						throw new Exception ("Unknown instruction: " + instruction);
					}
					
					if (transformed == null) {
						InstructionMap.Add (instruction, new Instruction [] { instruction });
						
					} else {
						var list = transformed.ToList ();
						/*
						if (list.Count != 0 && list [0] != null && (list [0] != instruction || list.Count > 1)) {
								MapInstructions (method.Body, instruction, list [0], list [list.Count - 1]);
						}
						*/
						InstructionMap.Add (instruction, list);
					}
				} // foreach instruction
			} // foreach block
		}
		
		protected virtual void MapInstructions (Instruction original, Instruction newInst)
		{
			// FIXME: Inefficient
			var e = InstructionMap.Values.AsEnumerable ().Cast <IEnumerable<Instruction>> ().Flatten ();
			e.RedirectJumps (original, newInst);

			IList<Instruction> list;
			if (InstructionMap.TryGetValue (original, out list) && list.Count > 0)
				e.RedirectJumps (list [0], newInst);

			newInst.SequencePoint = original.SequencePoint;
		}
		
		protected virtual void MapAllInstructions (Mono.Cecil.Cil.MethodBody body)
		{
			for (var i = 0; i < InstructionMap.Count; i++) {
				
				var key  = InstructionMap.KeyForIndex (i);
				var list = InstructionMap [i];
				
				if (list.Count != 0 && list [0] != null && list [0] != key) {
					MapInstructions (key, list [0]);
					
				} else if (list.Count == 0) {
					
					for (var j = i; j < InstructionMap.Count; j++) {
						
						if (InstructionMap [j].Count != 0) {
							MapInstructions (key, InstructionMap [j] [0]);
							break;
						}
					}
				}
			}
		}
		
		// first is inclusive, last is exclusive
		protected virtual IList<Instruction> ExtractInstructionRange (Instruction first, Instruction last)
		{
			var key = first;
			var i = InstructionMap.IndexOfKey (key);
			var list = new List<Instruction> ();
			
			do {
				
				var current = InstructionMap [i];
				
				foreach (var replacement in current)
					list.Add (replacement);
				
				if (!current.IsReadOnly)
					current.Clear ();
				else
					InstructionMap [i] = new Instruction [0];
				
				key = InstructionMap.KeyForIndex (++i);
				
			} while (key != last);
			
			return list;
		}
		
		protected void InsertLdarg0BeforeLastStackItemAt (Instruction instruction)
		{
			foreach (var lastStack in cfg.FindLastStackItem (block, instruction, IsBarrier))
				InsertBefore (lastStack, Instruction.Create (OpCodes.Ldarg_0));
		}
		
		
		// -----
		
		public void InsertBefore (Instruction oldInstruction, params Instruction [] newInstructions)
		{
			InsertBefore (oldInstruction, (IList<Instruction>)newInstructions);
		}
		
		public virtual void InsertBefore (Instruction oldInstruction, IList<Instruction> newInstructions)
		{
			var list = InstructionMap [oldInstruction];
			if (list.Count != 0)
				MapInstructions (list [0], newInstructions [0]);

			if (list.IsReadOnly)
				list = new List<Instruction> (list);
			
			var i = 0;
			foreach (var newInstruction in newInstructions)
				list.Insert (i++, newInstruction);
			
			InstructionMap [oldInstruction] = list;
		}
		
		public virtual void InsertAfter (Instruction oldInstruction, IList<Instruction> newInstructions)
		{
			var list = InstructionMap [oldInstruction];			
			if (list.IsReadOnly)
				list = new List<Instruction> (list);
			
			foreach (var newInstruction in newInstructions)
				list.Add (newInstruction);
			
			InstructionMap [oldInstruction] = list;
		}
		
		public virtual IEnumerable<Instruction> OnNop (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBreak (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdarg_0 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdarg_1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdarg_2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdarg_3 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdloc_0 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdloc_1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdloc_2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdloc_3 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStloc_0 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStloc_1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStloc_2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStloc_3 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdarg (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdarga (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStarg (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdloc (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdloca (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStloc (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdnull (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_M1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_0 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_3 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_5 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_6 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_7 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4_8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_I8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_R4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdc_R8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnDup (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnPop (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnJmp (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCall (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCalli (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnRet (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBr (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBrfalse (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBrtrue (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBeq (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBge (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBgt (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBle (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBlt (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBne_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBge_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBgt_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBle_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBlt_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnSwitch (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_I1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_U1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_I2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_U2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_I4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_U4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_I8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_I (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_R4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_R8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdind_Ref (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_Ref (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_I1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_I2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_I4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_I8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_R4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_R8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnAdd (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnSub (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnMul (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnDiv (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnDiv_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnRem (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnRem_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnAnd (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnOr (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnXor (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnShl (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnShr (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnShr_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnNeg (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnNot (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_I1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_I2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_I4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_I8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_R4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_R8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_U4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_U8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCallvirt (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCpobj (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdobj (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdstr (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnNewobj (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCastclass (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnIsinst (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_R_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnUnbox (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnThrow (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdfld (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdflda (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStfld (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdsfld (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdsflda (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStsfld (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStobj (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I1_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I2_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I4_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I8_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U1_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U2_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U4_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U8_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnBox (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnNewarr (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdlen (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelema (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_I1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_U1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_I2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_U2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_I4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_U4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_I8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_I (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_R4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_R8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_Ref (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_I (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_I1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_I2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_I4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_I8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_R4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_R8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_Ref (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdelem_Any (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStelem_Any (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnUnbox_Any (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U4 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U8 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnRefanyval (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCkfinite (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnMkrefany (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdtoken (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_U2 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_U1 (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_I (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_I (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_Ovf_U (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnAdd_Ovf (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnAdd_Ovf_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnMul_Ovf (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnMul_Ovf_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnSub_Ovf (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnSub_Ovf_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnEndfinally (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLeave (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnStind_I (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnConv_U (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnArglist (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCeq (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCgt (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCgt_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnClt (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnClt_Un (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdftn (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLdvirtftn (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnLocalloc (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnEndfilter (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnUnaligned (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnVolatile (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnTail (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnInitobj (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnCpblk (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnInitblk (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnRethrow (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnSizeof (Instruction instruction)
		{
			return null;
		}

		public virtual IEnumerable<Instruction> OnRefanytype (Instruction instruction)
		{
			return null;
		}
		
		public virtual IEnumerable<Instruction> OnConstrained (Instruction instruction)
		{
			return null;
		}
	}
}
