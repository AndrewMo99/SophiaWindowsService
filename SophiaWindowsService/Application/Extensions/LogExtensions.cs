using System;
using System.Diagnostics;
using System.IO;

namespace SophiaWindowsService.Application.Extensions
{
    public static class LogExtensions
    {
        private static string LogPath => ConfigExtensions.LogPath;

        public static void WriteLog(this string message)
        {
            try
            {
                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);

                var logFile = Path.Combine(LogPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

                File.AppendAllText(logFile, logMessage + Environment.NewLine + Environment.NewLine);
            }
            catch
            {
                // ignored
            }
        }

        public static string GetErrorMessage(this Exception ex)
        {
            return (string.IsNullOrWhiteSpace(ex.InnerException?.ToString()) ? string.Empty : ex.InnerException?.ToString()) + " "
                + (string.IsNullOrWhiteSpace(ex.Message) ? string.Empty : ex.Message)
                + " "
                + (string.IsNullOrWhiteSpace(ex.StackTrace) ? string.Empty : ex.StackTrace)
                + " "
                + (string.IsNullOrWhiteSpace(ex.TargetSite?.ToString()) ? string.Empty : ex.TargetSite?.ToString())
                + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
        }

        public static void WriteEventLog(this string message, EventLogEntryType entryType)
        {
            if (!EventLog.SourceExists(ConfigExtensions.ServiceName))
                EventLog.CreateEventSource(ConfigExtensions.ServiceName, ConfigExtensions.EventLog);

            EventLog.WriteEntry(
                ConfigExtensions.ServiceName,
                message,
                entryType);
        }
    }
}