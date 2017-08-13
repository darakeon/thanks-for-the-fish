using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Git : Terminal
	{
		private readonly String sourceDirectory;
		public const String DEFAULT_BRANCH = "master";
		private String currentBranch = DEFAULT_BRANCH;
		private readonly IList<String> branches = new List<String> { DEFAULT_BRANCH };
		private String alreadyCommitedFile => Path.Combine(sourceDirectory, ".git", "hg-already-commited.txt");

		public Git(String sourceDirectory) : base(sourceDirectory)
		{
			this.sourceDirectory = sourceDirectory;
		}
		
		internal delegate Boolean AskOverwrite();
		internal delegate void NotifyNewCount(Int32 total, Int32 diff);

		public IList<Commit> Init(AskOverwrite askOverwrite, NotifyNewCount notifyNewCount, IList<Commit> commitList)
		{
			var gitConfig = Path.Combine(sourceDirectory, ".git");

			if (Directory.Exists(gitConfig))
			{
				var shouldOverwrite = askOverwrite();

				if (!shouldOverwrite)
				{
					var oldCount = commitList.Count;
					var alreadyCommited = getAlreadyCommited(commitList);

					if (alreadyCommited != null) notifyNewCount(oldCount, alreadyCommited.Count);

					return alreadyCommited;
				}

				Directory.Delete(gitConfig, true);
			}

			Run("git", "init");
			File.WriteAllText(alreadyCommitedFile, String.Empty);
			return null;
		}

		private IList<Commit> getAlreadyCommited(IList<Commit> commitList)
		{
			if (!File.Exists(alreadyCommitedFile)) return null;

			var commitedHashList = File.ReadAllLines(alreadyCommitedFile);
			var result = new List<Commit>();

			foreach (var commitedHash in commitedHashList)
			{
				var hashs = commitedHash.Split('|');
				var hg = hashs[0];

				var commit = commitList.Single(c => c.HgHash == hg);
				result.Add(commit);

				if (hashs.Length > 1)
				{
					commit.GitHash = hashs[1];
				}
			}

			return result;
		}

		public void RemakeIgnore()
		{
			var hgIgnore = Path.Combine(sourceDirectory, ".hgignore");

			var ignoreContent =
				File.Exists(hgIgnore)
					? File.ReadAllLines(hgIgnore).ToList()
					: new List<String>();

			ignoreContent.Add(".hg*");

			var gitIgnore = Path.Combine(sourceDirectory, ".gitignore");
            File.WriteAllLines(gitIgnore, ignoreContent);
		}

		public void HandleBranch(Commit commit)
		{
			if (commit.Branch == currentBranch) return;
			
			if (branches.Contains(commit.Branch))
			{
				Run("git", "checkout", commit.Branch);
			}
			else
			{
				Run("git", "checkout", "-b", commit.Branch);
				branches.Add(commit.Branch);
			}

			currentBranch = commit.Branch;
		}

		public void AddAndCommit(Commit commit)
		{
			Run("git", "add .");

			var date = commit.DateTime.ToString(@"yyyy-MM-dd HH:mm:ss (K)");
			var message = commit.Message
				+ Environment.NewLine + Environment.NewLine
				+ $"by [{commit.Author}] in [{date}] - hg hash {{{commit.HgHash}}}";
			
			var commited = this.commit(message);

			if (commited)
			{
				var result = Run("git", "rev-parse", "HEAD");
				commit.GitHash = result.Output.Trim();
			}

			if (!String.IsNullOrEmpty(commit.Tag))
			{
				Run("git", "tag", "-a", $@"""{commit.Tag}""", $@"-m ""{commit.Tag}""");
			}

			File.AppendAllLines(alreadyCommitedFile, new [] { $"{commit.HgHash}|{commit.GitHash}" });
		}

		public void CommitReversal(Commit commit)
		{
			if (String.IsNullOrEmpty(commit.GitHash)) return;

			Run("git", "revert", commit.GitHash, "-n");
			var message = $"This commit reverts {commit.GitHash} and should be squashed where hg joined it back to the tree.";
			this.commit(message);
		}

		private Boolean commit(String message)
		{
			try
			{
				Run("git", "commit", $@"-m ""{message.Replace(@"""", "'")}""", "-q");
				return true;
			}
			catch (TerminalException e)
			{
				var hasError = !String.IsNullOrEmpty(e.Result.Error);
				var emptyCommitAnswer = $"On branch master{Environment.NewLine}nothing to commit, working tree clean";
                var isEmptyCommit = e.Result.Output.Trim() == emptyCommitAnswer;

				if (hasError || !isEmptyCommit) throw;

				return false;
			}
		}
	}
}