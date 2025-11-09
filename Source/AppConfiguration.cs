namespace FolderSync.Source
{
    public class AppConfiguration
    {
        public string SourcePath { get; private set; }
        public string ReplicaPath { get; private set; }
        public int SyncInterval { get; private set; }
        public string LogPath { get; private set; }

        private AppConfiguration(string sourcePath, string replicaPath, int syncInterval, string logPath) 
        {
            SourcePath = Path.GetFullPath(sourcePath);
            ReplicaPath = Path.GetFullPath(replicaPath);
            SyncInterval = syncInterval;
            LogPath = Path.GetFullPath(logPath);
        }

        /// <summary>
        /// Parses and validates command-line arguments required for the application.
        /// </summary>
        /// <param name="args">
        /// An array of command-line arguments provided to the application, expected in the following order:
        /// <list type="number">
        /// <item><description><c>sourcePath</c> – The path to the source directory.</description></item>
        /// <item><description><c>replicaPath</c> – The path to the replica directory.</description></item>
        /// <item><description><c>syncInterval</c> – The synchronization interval (in seconds). Must be a positive integer.</description></item>
        /// <item><description><c>logPath</c> – The full path to the log file.</description></item>
        /// </list>
        /// </param>
        /// <returns>A configured <see cref="AppConfiguration"/> instance containing validated parameters.</returns>
        /// <exception cref="ArgumentException">Thrown when the number of arguments is incorrect or the synchronization interval is invalid.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the specified source directory does not exist.</exception>
        public static AppConfiguration Parse(string[] args)
        {
            if (args.Length != 4)
                throw new ArgumentException("The following arguments are required in order: <sourceFolder> <destinationFolder> <syncIntervalSeconds> <logFilePath>");

            string sourcePath = args[0];
            string replicaPath = args[1];
            string logPath = args[3];

            if (!int.TryParse(args[2], out int syncInterval) || syncInterval <= 0)
                throw new ArgumentException($"Synchronization interval must be a positive integer. Got: {syncInterval}");

            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"Source folder '{sourcePath}' does not exist.");

            if (!Directory.Exists(replicaPath))
            {
                Directory.CreateDirectory(replicaPath);
            }

            string? logFileDirectory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(logFileDirectory) && !string.IsNullOrEmpty(logFileDirectory))
                Directory.CreateDirectory(logFileDirectory!);

            return new(sourcePath, replicaPath, syncInterval, logPath);
        }
    }
}
