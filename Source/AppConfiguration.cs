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
                Console.WriteLine($"Replica folder '{replicaPath}' does not exist. Creating...");
                Directory.CreateDirectory(replicaPath);
            }

            string? logFileDirectory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(logFileDirectory) && !string.IsNullOrEmpty(logFileDirectory))
                Directory.CreateDirectory(logFileDirectory!);

            return new(sourcePath, replicaPath, syncInterval, logPath);
        }
    }
}
