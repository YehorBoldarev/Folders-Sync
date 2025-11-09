using FolderSync.Source;
using Serilog;

namespace FolderSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.BasicInitialization();

            try
            {
                var config = AppConfiguration.Parse(args);

                DirectoriesComparer dirComparer = new(config.SourcePath, config.ReplicaPath);
                Logger.Configure(config.LogPath);

                Log.Information("Application started.");
                Log.Information("Source: {Source}", config.SourcePath);
                Log.Information("Replica: {Destination}", config.ReplicaPath);
                Log.Information("Sync Interval: {Interval} seconds", config.SyncInterval);
                Log.Information("Log file: {Log}", config.LogPath);

                while (true)
                {
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

                    Thread.Sleep(TimeSpan.FromSeconds(config.SyncInterval));
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
    }
}