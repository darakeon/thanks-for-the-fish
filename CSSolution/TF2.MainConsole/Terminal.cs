using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TF2.MainConsole
{
	public static class Terminal
	{
		public static Result Run(String directory, Encoding encoding, String command, params String[] args)
		{
			return run(directory, encoding, command, args, false);
		}

		public static Result Run(String directory, String command, params String[] args)
		{
			return run(directory, null, command, args, false);
		}

		public static Result RunAsAdm(String directory, String command, params String[] args)
		{
			return run(directory, null, command, args, true);
		}

		private static Result run(String directory, Encoding encoding, String command, String[] args, Boolean requestAdm)
		{
			var joinedArgs = String.Join(" ", args);

			var proc = createProcess(directory, command, joinedArgs, requestAdm, encoding);

			if (requestAdm)
			{
				proc.StartInfo.Verb = "runas";
			}

			proc.Start();

			var result = new Result();
			result.Process(proc);

			return result;
		}

		private static Process createProcess(String directory, String command, String joinedArgs, Boolean requestAdm, Encoding encoding)
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
