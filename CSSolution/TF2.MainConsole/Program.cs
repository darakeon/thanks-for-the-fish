using System;

namespace TF2.MainConsole
{
	internal class Program
	{
		public static void Main(String[] args)
		{
			var sourceDirectory = args[0];

			var hgLog = Terminal.Run(sourceDirectory, "hg", "log");
			Console.WriteLine(hgLog.Output);

			Console.Read();


		}
	}
}
