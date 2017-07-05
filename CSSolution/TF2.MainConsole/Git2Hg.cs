using System;
using System.Collections.Generic;

namespace TF2.MainConsole
{
	class Git2Hg
	{
		public static Boolean CommitOnGit(String sourceDirectory, IList<Commit> commitList, Func<String, Boolean> askCommit, Action<Terminal.Result> showUpdateError)
		{
			foreach (var commit in commitList)
			{
				var hgUpdate = Hg.Update(sourceDirectory, commit);

				if (hgUpdate.Succedded)
				{
					var position = commitList.IndexOf(commit) + 1;
					var title = $"[{position}/{commitList.Count}] {commit.Hash}: {commit.Message}";

					var shouldCommit = askCommit(title);
					if (!shouldCommit) return false;

					Git.RemakeIgnore(sourceDirectory);
					Git.AddAndCommit(sourceDirectory, commit);
				}
				else
				{
					showUpdateError(hgUpdate);
					return false;
				}
			}

			return true;
		}
	}
}