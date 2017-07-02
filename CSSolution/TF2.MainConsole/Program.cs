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

			File.ReadAllLines("disclaimer.txt")
				.ToList().ForEach(Console.WriteLine);

			var answer = ask(() => Console.WriteLine("Do you agree (y/n)?"), "y", "n");

			if (answer.ToLower() == "n")
			{
				Console.WriteLine("Ok. See ya! o/");
			}
			else
			{
			}

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

		private static string ask(Action question, params String[] acceptedAnswers)
		{
			String answer;

			do
			{
				question();
				answer = Console.ReadLine()?.ToLower();
			}
			while (!acceptedAnswers.Contains(answer));

			return answer;
		}
	}
}
