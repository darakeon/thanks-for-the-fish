using System;
using System.Collections.Generic;

namespace TF2.MainConsole
{
	internal class Git2Hg
	{
		private readonly Hg hg;
		private readonly Git git;

		public Int32 CommitCount => commitList?.Count ?? 0;
		private IList<Commit> commitList;

		public Git2Hg(String sourceDirectory)
		{
			hg = new Hg(sourceDirectory);
			git = new Git(sourceDirectory);
		}

		public Boolean PopulateCommitList(Hg.ShowSequenceError showSequenceError)
		{
			commitList = hg.PopulateCommitList(showSequenceError);
			return commitList != null;
		}

		public Boolean CommitOnGit(Git.AskOverwrite askOverwriteGit, Git.NotifyNewCount notifyNewCount, ShowUpdateError showUpdateError, AskCommit askCommit)
		{
			git.Init(askOverwriteGit, notifyNewCount, commitList);

			for (var c = 0; c < commitList.Count; c++)
			{
				var commit = commitList[c];

				var hgUpdate = hg.Update(commit);
				if (!hgUpdate.Succedded)
				{
					showUpdateError(hgUpdate);
					return false;
				}
				
				var position = c + 1;
				var title = $"[{position}/{CommitCount}] {commit.Hash}: {commit.Message}";

				git.RemakeIgnore();

				var shouldCommit = askCommit(title);
				if (!shouldCommit)
				{
					return false;
				}

				git.HandleBranch(commit);
				git.AddAndCommit(commit);
			}

			return true;
		}

		public delegate Boolean AskCommit(String commitTitle);
		public delegate void ShowUpdateError(Terminal.Result errorResult);
	}
}