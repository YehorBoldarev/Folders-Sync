using FolderSync.Source;
using Serilog;

namespace FolderSync
{
    class Program
    {
        static bool _isRunning = true;
        static bool _forceSync = false;

        static void Main(string[] args)
        {
            Logger.BasicInitialization();

            try
            {
                var config = AppConfiguration.Parse(args);

                Logger.Configure(config.LogPath);

                Log.Information("Application started.");
                Log.Information("Source: {Source}", config.SourcePath);
                Log.Information("Replica: {Destination}", config.ReplicaPath);
                Log.Information("Sync Interval: {Interval} seconds", config.SyncInterval);
                Log.Information("Log file: {Log}", config.LogPath);

                var keyThread = new Thread(KeyControl);
                keyThread.IsBackground = true;
                keyThread.Start();

                Console.WriteLine("Usage note: Press 'Enter' or 'q' to exit. Press 'r' to force synchronization.");

                while (_isRunning)
                {
                    SynchronizationProcess(config);

                    WaitingLoop(config.SyncInterval);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"Synchronization failed to start: {ex.Message}");
            }
            finally
            {
                Log.Information("Application exited.");
                Logger.Close();
            }
        }

        static void SynchronizationProcess(AppConfiguration config)
        {
            DirectoriesComparer dirComparer = new(config.SourcePath, config.ReplicaPath);

            if (!Directory.Exists(config.SourcePath))
                throw new DirectoryNotFoundException($"Source directory ({config.SourcePath}) not found");

            if (!Directory.Exists(config.ReplicaPath))
            {
                Log.Warning($"Replica folder ({config.ReplicaPath}) was not found. Creating...");
                Directory.CreateDirectory(config.ReplicaPath);
            }

            Log.Information($"Synchronization started at {DateTime.Now}");

            var comparisonResult = dirComparer.Compare();
            
            foreach (string dirPath in comparisonResult.DirectoriesToAdd)
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(config.ReplicaPath, dirPath));
                    Log.Information($"Created {dirPath} directory in replica folder");

                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to create {dirPath} directory in replica folder: {ex.Message}");
                }
            }

            foreach(KeyValuePair<string, FileInfo> fileInfo in comparisonResult.FilesToAdd)
            {
                string destinationPath = Path.Combine(config.ReplicaPath, fileInfo.Key);

                try
                {
                    var creationTime = fileInfo.Value.CreationTime;
                    fileInfo.Value.CopyTo(destinationPath);
                    File.SetCreationTime(destinationPath, creationTime);
                    Log.Information($"Added {fileInfo.Key} to replica folder");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to add file {fileInfo.Key} to replica folder: {ex.Message}");
                } 
            }

            foreach(KeyValuePair<string, FileInfo> fileInfo in comparisonResult.FilesToUpdate)
            {
                string destinationPath = Path.Combine(config.ReplicaPath, fileInfo.Key);

                try
                {
                    var creationTime = fileInfo.Value.CreationTime;
                    fileInfo.Value.CopyTo(destinationPath, true);
                    File.SetCreationTime(destinationPath, creationTime);
                    Log.Information($"Updated {fileInfo.Key} in replica folder");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to update {fileInfo.Key} in replica folder: {ex.Message}");
                }
            }

            foreach (string dirPath in comparisonResult.DirectoriesToRemove)
            {
                try
                {
                    Directory.Delete(Path.Combine(config.ReplicaPath, dirPath), true);
                    Log.Information($"Deleted {dirPath} directory in replica folder");

                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to delete {dirPath} directory in replica folder: {ex.Message}");
                }
            }

            foreach (KeyValuePair<string, FileInfo> fileInfo in comparisonResult.FilesToRemove)
            {
                if (File.Exists(fileInfo.Value.FullName))
                {
                    try
                    {
                        fileInfo.Value.Delete();
                        Log.Information($"Deleted {fileInfo.Key} in replica folder");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to remove {fileInfo.Key} in replica folder: {ex.Message}");
                    }
                }
            }
        }

        static void WaitingLoop(int seconds)
        {
            DateTime endTime = DateTime.UtcNow.AddSeconds(seconds);

            while (DateTime.UtcNow < endTime && _isRunning && !_forceSync)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(5));
            }

            if (_forceSync)
                _forceSync = false;
        }

        static void KeyControl()
        {
            List<ConsoleKey> exitKeys = [ConsoleKey.Escape, ConsoleKey.Q];

            while (_isRunning)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (exitKeys.Contains(key.Key))
                    {
                        _isRunning = false;
                        Log.Information("Exiting...");
                    }
                    else if (key.Key == ConsoleKey.R)
                    {
                        _forceSync = true;
                        Log.Information("Trigerring forced synchronization...");
                    }
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        }
    }
}