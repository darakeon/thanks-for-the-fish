using System;
using System.Collections.Generic;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Program
	{
		public static void Main(String[] args)
		{
			var sourceDirectory = args[0];

			var commitList = getCommitList(sourceDirectory);
			if (commitList == null) return;

			Console.WriteLine(commitList.Count);
			commitList.ToList().ForEach(Console.WriteLine);

			Console.Read();
		}

		private static IList<Commit> getCommitList(String sourceDirectory)
		{
			var hgLog = Hg.GetLog(sourceDirectory);
			var commitList = Hg.GetCommitList(hgLog.Output);
			return Hg.VerifyCommitList(commitList, show);
		}

		private static void show(Int32 expected, Int32 received)
		{
			Console.WriteLine("Error on parsing commits:");
			Console.WriteLine($"Commit with position {received} in repository is in position {expected} in list.");
			Console.WriteLine($"Maybe the position {expected} in repository was not parsed.");
		}

	}
}
