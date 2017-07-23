using System;
using System.Collections.Generic;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Git2Hg
	{
		private readonly Hg hg;
		private readonly Git git;

		public Int32 CommitCount;
		private IList<Commit> commitList;

		public Git2Hg(String sourceDirectory)
		{
			hg = new Hg(sourceDirectory);
			git = new Git(sourceDirectory);
		}

		public Boolean PopulateCommitList(Hg.ShowSequenceError showSequenceError)
		{
			commitList = hg.PopulateCommitList(showSequenceError);
			CommitCount = commitList.Count;
			return commitList != null;
		}

		public Boolean CommitOnGit(Git.AskOverwrite askOverwriteGit, Git.NotifyNewCount notifyNewCount, AskCommit askCommit, WarnReversal warnReversal)
		{
			var alreadyCommited = git.Init(askOverwriteGit, notifyNewCount, commitList);
			var position = 0;
			CommitCount = commitList.Count - alreadyCommited.Count;

			foreach (var commit in commitList)
			{
				if (alreadyCommited.Contains(commit)) continue;

				position++;

				hg.Update(commit);
				git.RemakeIgnore();

				var title = $"[{position}/{CommitCount}] {commit.HgHash}: "
					+ $"({commit.DateTime.ToString("yyyy-MM-dd")}) {commit.Message}";
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
					).OrderByDescending(r => r.Position).ToList();

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