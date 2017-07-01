using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TF2.MainConsole
{
	public static class Terminal
	{
		public static Result Run(String directory, String command, params String[] args)
		{
			return run(directory, command, args, false);
		}

		public static Result RunAsAdm(String directory, String command, params String[] args)
		{
			return run(directory, command, args, true);
		}

		private static Result run(String directory, String command, String[] args, Boolean requestAdm)
		{
			var joinedArgs = String.Join(" ", args);

			var proc = new Process
			{
				StartInfo = new ProcessStartInfo(command, joinedArgs)
				{
					UseShellExecute = requestAdm,
					RedirectStandardError = !requestAdm,
					RedirectStandardInput = !requestAdm,
					RedirectStandardOutput = !requestAdm,
					WorkingDirectory = directory,
				}
			};

			if (requestAdm)
			{
				proc.StartInfo.Verb = "runas";
			}

			proc.Start();

			var result = new Result();
			result.Process(proc);

			return result;
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

		public static String ReadClean(this StreamReader reader)
		{
			var result = reader.ReadToEnd();
			result = Regex.Replace(result, @"\0", "");
			return result.Trim();
		}
	}
}
