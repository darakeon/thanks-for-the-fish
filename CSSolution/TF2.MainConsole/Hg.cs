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
		
		public IList<Commit> CommitList { get; private set; }

		public Hg(String sourceDirectory) : base(sourceDirectory) { }

		public Boolean PopulateCommitList(ShowSequenceError showSequenceError)
		{
			hgLog = Run(Encoding.UTF7, "hg", "log");

			parseCommitList();

			return validateCommitList(showSequenceError);
		}

		private void parseCommitList()
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

			var commitMatches = logRegex.Matches(hgLog.Output).Cast<Match>();
			
			CommitList = commitMatches
				.Select(m => getCommit(m.Groups))
				.OrderBy(c => c.Position).ToList();
		}

		private Commit getCommit(GroupCollection groups)
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
			for (var c = 0; c < CommitList.Count; c++)
			{
				var hgPosition = CommitList[c].Position;

				if (c == hgPosition) continue;

				showSequenceError(c, hgPosition);
				CommitList = null;
				return false;
			}

			return true;
		}

		public Result Update(Commit commit)
		{
			return Run("hg", "up", commit.Hash, "-C");
		}

		public delegate void ShowSequenceError(Int32 expected, Int32 received);
	}
}