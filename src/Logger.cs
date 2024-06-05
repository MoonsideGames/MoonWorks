using System;

namespace MoonWorks
{
	public static class Logger
	{
		public static Action<string> LogInfo = LogInfoDefault;
		public static Action<string> LogWarn = LogWarnDefault;
		public static Action<string> LogError = LogErrorDefault;

		private static void LogInfoDefault(string str)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("INFO: ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(str);
		}

		private static void LogWarnDefault(string str)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARN: ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(str);
		}

		private static void LogErrorDefault(string str)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR: ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(str);
		}

		private static void RefreshLogInfo(IntPtr msg)
		{
			LogInfo(UTF8_ToManaged(msg));
		}

		private static void RefreshLogWarn(IntPtr msg)
		{
			LogWarn(UTF8_ToManaged(msg));
		}

		private static void RefreshLogError(IntPtr msg)
		{
			LogError(UTF8_ToManaged(msg));
		}

		private unsafe static string UTF8_ToManaged(IntPtr s)
		{
			byte* ptr = (byte*) s;
			while (*ptr != 0)
			{
				ptr += 1;
			}

			string result = System.Text.Encoding.UTF8.GetString(
				(byte*) s,
				(int) (ptr - (byte*) s)
			);

			return result;
		}
	}
}
