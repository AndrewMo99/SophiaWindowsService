namespace SophiaWindowsService.Application.Extensions
{
    public static class ConfigExtensions
    {
        public static string ServiceName => "SophiaWindowsService";
        public static string DbConnectionName => "SophiaDbContext";
        public static string DbSchema => "dbo";
        public static string LogPath => @"C:\Users\Public\LogSophiaWindowsService\";
        public static string EventLog => "Application";
    }
}