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
		public static IList<Commit> GetCommitList(String sourceDirectory, ShowWrongPosition show)
		{
			var hgLog = getLog(sourceDirectory);
			var commitList = getCommitList(hgLog.Output);
			return verifyCommitList(commitList, show);
		}

		private static Terminal.Result getLog(String sourceDirectory)
		{
			return Terminal.Run(sourceDirectory, Encoding.UTF7, "hg", "log");
		}

		private static IList<Commit> getCommitList(String hgLog)
		{
			var datePattern = @"\w{3} \w{3} \d{2} \d{2}:\d{2}:\d{2} \d{4}( [+-]\d{4})?";

			var logPattern = 
				$@"changeset: +(\d+):([a-z0-9]+){Environment.NewLine}"+
				$@"(branch: +(.*){Environment.NewLine})?" +
				$@"(tag: +(.*){Environment.NewLine})?" +
				$@"(parent: +(.*){Environment.NewLine})?" +
				$@"(parent: +(.*){Environment.NewLine})?" +
				$@"user: +(.*){Environment.NewLine}" +
				$@"date: +({datePattern}).*{Environment.NewLine}" +
				@"(summary: +(.*))?";

			var logRegex = new Regex(logPattern);

			var commitMatches = logRegex.Matches(hgLog).Cast<Match>();
			var commitList = commitMatches.Select(m => getCommit(m.Groups));

			return commitList.OrderBy(c => c.Position).ToList();
		}

		private static Commit getCommit(GroupCollection groups)
		{
			return new Commit
			{
				Position = Int32.Parse(groups[1].Value),
				Hash = groups[2].Value,
				Tag = groups[6].Value,
				Author = groups[11].Value,
				DateTime = parseDate(groups[12].Value, groups[13].Value),
				Message = parseMessage(groups[15].Value),
			};
		}

		private static DateTime parseDate(String value, String gmt)
		{
			var hasGmt = String.IsNullOrEmpty(gmt);
            var format = "ddd MMM dd HH:mm:ss yyyy" + (hasGmt ? "" : " K");
			var culture = CultureInfo.InvariantCulture;
			return DateTime.ParseExact(value, format, culture);
		}

		private static String parseMessage(String value)
		{
			return Regex.Replace(value, "\r?\n", Environment.NewLine);
		}

		private static IList<Commit> verifyCommitList(IList<Commit> commitList, ShowWrongPosition showWrongPosition)
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