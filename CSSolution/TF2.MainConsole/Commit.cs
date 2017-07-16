using System;
using System.Collections.Generic;

namespace TF2.MainConsole
{
	internal class Commit
	{
		public Int32 Position { get; set; }
		public String HgHash { get; set; }
		public String GitHash { get; set; }

		private String branch;
		public String Branch
		{
			get { return branch; }
			set { branch = String.IsNullOrEmpty(value) ? Git.DEFAULT_BRANCH : value; }
		}

		private String tag;
		public String Tag
		{
			get { return tag; }
			set { tag = value == "tip" ? null : value; }
		}

		public List<Commit> ParentList { get; set; }
		public String Author { get; set; }
		public DateTime DateTime { get; set; }
		public String Message { get; set; }

		public override string ToString()
		{
			return $"{Position}:{HgHash}";
		}
		}
	}
}