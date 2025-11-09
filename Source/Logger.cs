using Serilog;

namespace FolderSync.Source
{
    /// <summary>
    /// Provides configuration and management for the application's logging using Serilog
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Configures full logging to both console and file output.
        /// </summary>
        public static void Configure(string logFilePath)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(logFilePath,
                              Serilog.Events.LogEventLevel.Information,
                              "[{Timestamp:yyyy-MM-dd HH:mm:ss:fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                              rollingInterval: RollingInterval.Day)
                .WriteTo.Console(Serilog.Events.LogEventLevel.Debug,
                                 "[{Timestamp:yyyy-MM-dd HH:mm:ss:fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        /// <summary>
        /// Configures a simple console logger for early startup or fallback logging.
        /// </summary>
        public static void BasicInitialization()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Console(                    
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss:fff}] [{Level:u5}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        public static void Close()
        {
            Log.CloseAndFlush();
        }
    }
}
