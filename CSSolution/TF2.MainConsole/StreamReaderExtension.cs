using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TF2.MainConsole
{
	public static class StreamReaderExtension
	{
		public static String ReadClean(this StreamReader reader)
		{
			var result = reader.ReadToEnd();
			result = Regex.Replace(result, @"\0", "");
			return result.Trim();
		}
	}
}