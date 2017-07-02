using System;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Program
	{
		public static void Main(String[] args)
		{
			var sourceDirectory = args[0];

			var hgLog = Terminal.Run(sourceDirectory, "hg", "log");
			
			var commitList = Hg.GetCommitList(hgLog.Output);

			Console.WriteLine(commitList.Count);
			commitList.ToList().ForEach(Console.WriteLine);

			Console.Read();
		}
	}
}
