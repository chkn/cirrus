using System;
using System.Collections.Generic;

namespace Cirrus.Tools.Cilc.Targets.Web {
	
	public class Scope {
		
		private static char [] parts;
		
		private int count;
		private Dictionary<object,string> name_bag;
		
		static Scope ()
		{
			parts = new char [26];
			for (int i = 0;  i <= 25; i++)
				parts [i] = (char)(i + 97);
		}
		
		public Scope ()
		{
			name_bag = new Dictionary<object, string> ();	
		}
		
		// FIXME
		public Scope Child ()
		{
			return this;	
		}
		
		public string GetName ()
		{
			var name = string.Empty;
			var i = count++;
			var max = parts.Length;
			
			do {
				name = parts [i % max] + name;
				i = (int)(Math.Floor ((double)(i / max))) - 1;
			} while (i >= 0);
			
			return name;
		}
		
		public string GetNameFor (object obj)
		{
			string name;
			if (!name_bag.TryGetValue (obj, out name)) {
				name = GetName ();
				name_bag.Add (obj, name);
			}
			return name;
		}
	}
}

