using System;
using System.Collections;
using System.Collections.Generic;

using Mono.Cecil;

namespace Cirrus.Tools.Cilc {
	public class Pipeline : Target, IList<Target> {
		
		private List<Target> targets = new List<Target> ();
		
		public override bool ProcessModule (ModuleDefinition module)
		{
			var transformed = false;
			
			foreach (var targ in targets)
				transformed |= targ.ProcessModule (module);
			
			return transformed;
		}
		
		public override void SaveOutput (ModuleDefinition module, string inputFileName)
		{
			targets [targets.Count - 1].SaveOutput (module, inputFileName);
		}
		
		public int IndexOf (Target item)
		{
			return targets.IndexOf (item);
		}

		public void Insert (int index, Target item)
		{
			targets.Insert (index, item);
		}

		public void RemoveAt (int index)
		{
			targets.RemoveAt (index);
		}

		public Target this[int index] {
			get { return targets [index]; }
			set { targets [index] = value; }
		}
		
		public IEnumerator<Target> GetEnumerator ()
		{
			return targets.GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public void Add (Target target)
		{
			targets.Add (target);
		}

		public void Clear ()
		{
			targets.Clear ();
		}

		public bool Contains (Target item)
		{
			return targets.Contains (item);
		}

		public void CopyTo (Target[] array, int arrayIndex)
		{
			targets.CopyTo (array, arrayIndex);
		}

		public bool Remove (Target item)
		{
			return targets.Remove (item);
		}

		public int Count {
			get { return targets.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}
	}
}

