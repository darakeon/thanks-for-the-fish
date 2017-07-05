using System;
using System.Diagnostics;
using System.Text;

namespace TF2.MainConsole
{
	public abstract class Terminal
	{
		private readonly String directory;

		protected Terminal(String directory)
		{
			this.directory = directory;
		}

        protected Result Run(Encoding encoding, String command, params String[] args)
		{
			return run(encoding, command, args, false);
		}

		protected Result Run(String command, params String[] args)
		{
			return run(null, command, args, false);
		}

		protected Result RunAsAdm(String command, params String[] args)
		{
			return run(null, command, args, true);
		}

		private Result run(Encoding encoding, String command, String[] args, Boolean requestAdm)
		{
			var joinedArgs = String.Join(" ", args);

			var proc = createProcess(command, joinedArgs, requestAdm, encoding);

			if (requestAdm)
			{
				proc.StartInfo.Verb = "runas";
			}

			proc.Start();

			var result = new Result();
			result.Process(proc);

			return result;
		}

		private Process createProcess(String command, String joinedArgs, Boolean requestAdm, Encoding encoding)
		{
			return new Process
			{
				StartInfo = new ProcessStartInfo(command, joinedArgs)
				{
					UseShellExecute = requestAdm,
					RedirectStandardInput = !requestAdm,
					RedirectStandardOutput = !requestAdm,
					RedirectStandardError = !requestAdm,
					StandardOutputEncoding = encoding,
					StandardErrorEncoding = encoding,
					WorkingDirectory = directory,
				}
			};
		}

		public class Result
		{
			internal void Process(Process proc)
			{
				var output = new StringBuilder();

				while (!proc.HasExited)
				{
					if (proc.StartInfo.RedirectStandardOutput)
					{
						output.AppendLine(proc.StandardOutput.ReadLine());
					}
				}

				Output = output.ToString();
				Error = proc.StandardError.ReadClean();
                Code = proc.ExitCode;
			}

			public Boolean Succedded => Code == 0;

			public Int32 Code { get; private set; }
			public String Output { get; private set; }
			public String Error { get; private set; }
		}
	}
}
