using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TF2.MainConsole
{
	internal class Hg
	{
		public static Terminal.Result GetLog(String sourceDirectory)
		{
			return Terminal.Run(sourceDirectory, Encoding.UTF7, "hg", "log");
		}

		public static IList<Commit> GetCommitList(String hgLog)
		{
			var logPattern = 
				$@"changeset: +(\d+):([a-z0-9]+){Environment.NewLine}"+
				$@"(tag: +(.*){Environment.NewLine})?" +
				$@"(parent: +(.*){Environment.NewLine})?" +
				$@"(parent: +(.*){Environment.NewLine})?" +
				$@"user: +(.*){Environment.NewLine}" +
				$@"date: +(.*){Environment.NewLine}" +
				@"summary: +(.*)";

			var logRegex = new Regex(logPattern);

			var commitMatches = logRegex.Matches(hgLog).Cast<Match>();

			return commitMatches
				.Select(m => getCommit(m.Groups))
				.OrderBy(c => c.Position)
				.ToList();
		}

		private static Commit getCommit(GroupCollection groups)
		{
			return new Commit
			{
				Position = Int32.Parse(groups[1].Value),
				Hash = groups[2].Value,
				Tag = groups[4].Value,
				Author = groups[9].Value,
				DateTime = parseDate(groups[10].Value),
				Message = parseMessage(groups[11].Value),
			};
		}

		private static DateTime parseDate(String value)
		{
			var format = "ddd MMM dd HH:mm:ss yyyy K";
			var culture = CultureInfo.InvariantCulture;
			return DateTime.ParseExact(value, format, culture);
		}

		private static String parseMessage(String value)
		{
			return Regex.Replace(value, "\r?\n", Environment.NewLine);
		}

		public static IList<Commit> VerifyCommitList(IList<Commit> commitList, ShowWrongPosition showWrongPosition)
		{
			for (var c = 0; c < commitList.Count; c++)
			{
				var hgPosition = commitList[c].Position;

				if (c != hgPosition)
				{
					showWrongPosition(c, hgPosition);
					return null;
				}
			}

			return commitList;
		}

		public delegate void ShowWrongPosition(Int32 expected, Int32 received);

		public static Terminal.Result Update(String sourceDirectory, Commit commit)
		{
			return Terminal.Run(sourceDirectory, "hg", "up", commit.Hash);
		}
	}
}