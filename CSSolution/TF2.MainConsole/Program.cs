﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Program
	{
		public static void Main(String[] args)
		{
			var sourceDirectory = args[0];

			processCommits(sourceDirectory);

			Console.Read();
		}

		private static void processCommits(String sourceDirectory)
		{
			var commitList = getCommitList(sourceDirectory);
			if (commitList == null) return;

			File.ReadAllLines("disclaimer.txt")
				.ToList().ForEach(Console.WriteLine);

			var answer = ask(() =>
			{
				Console.WriteLine();
				Console.WriteLine($"Total commits: {commitList.Count}");
				Console.Write("Do you want to go ahead? (y/n) ");
			}, "y", "n");

			if (answer.ToLower() == "n")
			{
				Console.WriteLine();
				Console.WriteLine("Ok. See ya! o/");
				return;
			}
			
			var succeded = commitOnGit(sourceDirectory, commitList);
			if (!succeded) return;

			Console.WriteLine();
			Console.WriteLine($"Oh, my god! All {commitList.Count} commits done!");
			Console.WriteLine("Mercurial, farewell and thanks for the fish!");
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

		private static Boolean commitOnGit(String sourceDirectory, IList<Commit> commitList)
		{
			foreach (var commit in commitList)
			{
				var hgUpdate = Hg.Update(sourceDirectory, commit);

				if (hgUpdate.Succedded)
				{
					var position = commitList.IndexOf(commit) + 1;
					var title = $"[{position}/{commitList.Count}] {commit.Hash}: {commit.Message}";

					var answer = ask(() =>
					{
						Console.WriteLine();
						Console.ForegroundColor = ConsoleColor.White;
						Console.WriteLine(title);
						Console.ResetColor();
						Console.Write("Commit on git? (y/n) ");
					}, "y", "n");

					if (answer.ToLower() == "n")
					{
						Console.WriteLine("Process stopped. You've been warned.");
						return false;
					}

					Git.RemakeIgnore(sourceDirectory);
					Git.AddAndCommit(sourceDirectory, commit);
				}
				else
				{
					Console.WriteLine("Sorry, we cannot progress, a problem occured with the update.");
					Console.WriteLine(hgUpdate.Error ?? hgUpdate.Output);
					return false;
				}
			}

			return true;
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
