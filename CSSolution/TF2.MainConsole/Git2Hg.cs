using System;
using System.Collections.Generic;
using System.Linq;

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

		public Boolean CommitOnGit(Git.AskOverwrite askOverwriteGit, Git.NotifyNewCount notifyNewCount, AskCommit askCommit, WarnReversal warnReversal)
		{
			git.Init(askOverwriteGit, notifyNewCount, commitList);

			for (var c = 0; c < commitList.Count; c++)
			{
				var commit = commitList[c];

				hg.Update(commit);
				
				var position = c + 1;
				var title = $"[{position}/{CommitCount}] {commit.HgHash}: {commit.Message}";

				git.RemakeIgnore();

				var shouldCommit = askCommit(title);
				if (!shouldCommit)
				{
					return false;
				}

				git.HandleBranch(commit);
				git.AddAndCommit(commit);

				var nextInBranch = 
					commitList.FirstOrDefault(
						n => n.Position > commit.Position 
							&& n.Branch == commit.Branch
					);
				
				if (nextInBranch != null && !nextInBranch.IsChildOf(commit))
				{
					var notRevert = nextInBranch.ParentList.Single();

					var revertList = commitList.Where(
						r => r.Position > notRevert.Position 
							&& r.Position <= commit.Position
					).ToList();

					revertList.ForEach(git.CommitReversal);
					warnReversal();
				}
			}

			return true;
		}

		public delegate Boolean AskCommit(String commitTitle);
		public delegate void WarnReversal();
	}
}