using System;

namespace TF2.MainConsole
{
	internal class Git2Hg
	{
		private readonly Hg hg;
		private readonly Git git;

		public Int32 CommitCount => hg?.CommitList?.Count ?? 0;

		public Git2Hg(String sourceDirectory)
		{
			hg = new Hg(sourceDirectory);
			git = new Git(sourceDirectory);
		}

		public Boolean PopulateCommitList(Hg.ShowSequenceError showSequenceError)
		{
			return hg.PopulateCommitList(showSequenceError);
		}

		public Boolean CommitOnGit(ShowUpdateError showUpdateError, AskCommit askCommit)
		{
			for (var c = 0; c < hg.CommitList.Count; c++)
			{
				var commit = hg.CommitList[c];

				var hgUpdate = hg.Update(commit);
				if (!hgUpdate.Succedded)
				{
					showUpdateError(hgUpdate);
					return false;
				}
				
				var position = c + 1;
				var title = $"[{position}/{CommitCount}] {commit.Hash}: {commit.Message}";

				var shouldCommit = askCommit(title);
				if (!shouldCommit)
				{
					return false;
				}

				git.RemakeIgnore();
				git.AddAndCommit(commit);
			}

			return true;
		}

		public delegate Boolean AskCommit(String commitTitle);
		public delegate void ShowUpdateError(Terminal.Result errorResult);
	}
}