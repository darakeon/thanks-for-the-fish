using System;
using System.IO;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Git
	{
		public static void RemakeIgnore(string sourceDirectory)
		{
			var hgIgnore = Path.Combine(sourceDirectory, ".hgignore");
			var ignoreContent = File.ReadAllLines(hgIgnore).ToList();

			ignoreContent.Add(".hg*");

			var gitIgnore = Path.Combine(sourceDirectory, ".gitignore");
            File.WriteAllLines(gitIgnore, ignoreContent);
		}

		public static void AddAndCommit(String sourceDirectory, Commit commit)
		{
			Terminal.Run(sourceDirectory, "git", "add .");
			Terminal.Run(sourceDirectory, "git", "commit", $@"-m ""{commit.Message}""");
		}
	}
}