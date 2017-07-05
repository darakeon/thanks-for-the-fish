using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Git : Terminal
	{
		private readonly string sourceDirectory;

		public Git(String sourceDirectory) : base(sourceDirectory)
		{
			this.sourceDirectory = sourceDirectory;
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

		public void AddAndCommit(Commit commit)
		{
			Run("git", "add .");

			var date = commit.DateTime.ToString(@"yyyy-MM-dd HH:mm:ss (K)");
			var message = commit.Message
				+ Environment.NewLine + Environment.NewLine
				+ $"by [{commit.Author}] in [{date}]";

            Run("git", "commit", $@"-m ""{message}""", "-q");

			if (!String.IsNullOrEmpty(commit.Tag))
			{
				Run("git", "tag", commit.Tag);
			}
		}

		internal delegate Boolean AskOverwrite();

		public void Init(AskOverwrite askOverwrite)
		{
			var gitConfig = Path.Combine(sourceDirectory, ".git");

			if (Directory.Exists(gitConfig))
			{
				var shouldOverwrite = askOverwrite();
				if (!shouldOverwrite) return;
				Directory.Delete(gitConfig, true);
			}

			Run("git", "init");
		}
	}
}