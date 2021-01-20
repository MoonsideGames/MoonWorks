using System;
using RefreshCS;

namespace MoonWorks
{
    public static class Logger
    {
        public static Action<string> LogInfo;
        public static Action<string> LogWarn;
        public static Action<string> LogError;

        private static RefreshCS.Refresh.Refresh_LogFunc LogInfoFunc = RefreshLogInfo;
        private static RefreshCS.Refresh.Refresh_LogFunc LogWarnFunc = RefreshLogWarn;
        private static RefreshCS.Refresh.Refresh_LogFunc LogErrorFunc = RefreshLogError;

        internal static void Initialize()
        {
            if (Logger.LogInfo == null)
            {
                Logger.LogInfo = Console.WriteLine;
            }
            if (Logger.LogWarn == null)
            {
                Logger.LogWarn = Console.WriteLine;
            }
            if (Logger.LogError == null)
            {
                Logger.LogError = Console.WriteLine;
            }

            Refresh.Refresh_HookLogFunctions(
                LogInfoFunc,
                LogWarnFunc,
                LogErrorFunc
            );
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
