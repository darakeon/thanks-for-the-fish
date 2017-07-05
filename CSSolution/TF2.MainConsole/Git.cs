using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Git
	{
		private readonly string sourceDirectory;

		public Git(String sourceDirectory)
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
			Terminal.Run(sourceDirectory, "git", "add .");

			var date = commit.DateTime.ToString(@"yyyy-MM-dd HH:mm:ss (K)");
			var message = commit.Message
				+ Environment.NewLine + Environment.NewLine
				+ $"by [{commit.Author}] in [{date}]";

            Terminal.Run(sourceDirectory, "git", "commit", $@"-m ""{message}""", "-q");

			if (!String.IsNullOrEmpty(commit.Tag))
			{
				Terminal.Run(sourceDirectory, "git", "tag", commit.Tag);
			}
		}
	}
}