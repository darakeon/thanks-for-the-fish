using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TF2.MainConsole
{
	internal class Hg : Terminal
	{
		private Result hgLog;
		private readonly IList<Commit> commitList = new List<Commit>();

		public Hg(String sourceDirectory) : base(sourceDirectory) { }

		public IList<Commit> PopulateCommitList(ShowSequenceError showSequenceError)
		{
			hgLog = Run(Encoding.UTF7, "hg", "log");

			parseCommitList();

			var isValid = validateCommitList(showSequenceError);

			return isValid ? commitList : null;
		}

		private void parseCommitList()
		{
			var datePattern = @"\w{3} \w{3} \d{2} \d{2}:\d{2}:\d{2} \d{4}( [+-]\d{4})?";

			var logPattern = 
				$@"changeset: +(\d+):([a-z0-9]+){Environment.NewLine}"+
				$@"(branch: +(.*){Environment.NewLine})?" +
				$@"(tag: +(.*){Environment.NewLine})?" +
				$@"(parent: +(\d+):([a-z0-9]+){Environment.NewLine})?" +
				$@"(parent: +(\d+):([a-z0-9]+){Environment.NewLine})?" +
				$@"user: +(.*){Environment.NewLine}" +
				$@"date: +({datePattern}).*{Environment.NewLine}" +
				@"(summary: +(.*))?";

			var logRegex = new Regex(logPattern);

			var commitMatches = 
				logRegex.Matches(hgLog.Output)
					.Cast<Match>()
					.OrderBy(m => Int32.Parse(m.Groups[1].Value));

			foreach (var commitMatch in commitMatches)
			{
				var commit = getCommit(commitMatch.Groups);
				commitList.Add(commit);
			}
		}

		private Commit getCommit(GroupCollection groups)
		{
			var parentHashList = new List<String> 
				{groups[9].Value, groups[12].Value}
				.Where(p => p != null).ToList();

			var parentList = commitList
				.Where(c => parentHashList.Contains(c.HgHash))
				.ToList();

			return new Commit
			{
				Position = Int32.Parse(groups[1].Value),
				HgHash = groups[2].Value,
				Branch = groups[4].Value,
				Tag = groups[6].Value,
				ParentList = parentList,
				Author = groups[13].Value,
				DateTime = parseDate(groups[14].Value, groups[15].Value),
				Message = parseMessage(groups[17].Value),
			};
		}

		private DateTime parseDate(String value, String gmt)
		{
			var hasGmt = String.IsNullOrEmpty(gmt);
            var format = "ddd MMM dd HH:mm:ss yyyy" + (hasGmt ? "" : " K");
			var culture = CultureInfo.InvariantCulture;
			return DateTime.ParseExact(value, format, culture);
		}

		private String parseMessage(String value)
		{
			return Regex.Replace(value, "\r?\n", Environment.NewLine);
		}

		private Boolean validateCommitList(ShowSequenceError showSequenceError)
		{
			for (var c = 0; c < commitList.Count; c++)
			{
				var hgPosition = commitList[c].Position;

				if (c == hgPosition) continue;

				showSequenceError(c, hgPosition);
				return false;
			}

			return true;
		}

		public void Update(Commit commit)
		{
			Run("hg", "up", commit.HgHash, "-C");
		}

		public delegate void ShowSequenceError(Int32 expected, Int32 received);
	}
}