using FolderSync.Source;

namespace FolderSync
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = AppConfiguration.Parse(args);

                DirectoriesComparer dirComparer = new(config.SourcePath, config.ReplicaPath);

                while (true)
                {
                    if (!Directory.Exists(config.SourcePath))
                        throw new DirectoryNotFoundException($"Source directory ({config.SourcePath}) not found");

                    if (!Directory.Exists(config.ReplicaPath))
                    {
                        Console.WriteLine($"Replica folder ({config.ReplicaPath}) was not found. Creating...");
                        Directory.CreateDirectory(config.ReplicaPath);
                    }

                    Console.WriteLine($"[INFO] Synchronization started at {DateTime.Now}");

                    var comparisonResult = dirComparer.Compare();
                    
                    foreach (string dirPath in comparisonResult.DirectoriesToAdd)
                    {
                        try
                        {
                            Console.WriteLine($"[INFO] Creating {dirPath} directory in replica folder...");
                            Directory.CreateDirectory(Path.Combine(config.ReplicaPath, dirPath));
                            Console.WriteLine($"[SUCCESS] Created {dirPath} directory in replica folder");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to create {dirPath} directory in replica folder: {ex.Message}");
                        }
                    }

                    foreach(KeyValuePair<string, FileInfo> fileInfo in comparisonResult.FilesToAdd)
                    {
                        string destinationPath = Path.Combine(config.ReplicaPath, fileInfo.Key);

                        try
                        {
                            Console.WriteLine($"[INFO] Adding {fileInfo.Key} to replica folder...");
                            var creationTime = fileInfo.Value.CreationTime;
                            fileInfo.Value.CopyTo(destinationPath);
                            File.SetCreationTime(destinationPath, creationTime);
                            Console.WriteLine($"[SUCCESS] Added {fileInfo.Key} to replica folder");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to add file {fileInfo.Key} to replica folder: {ex.Message}");
                        } 
                    }

                    foreach(KeyValuePair<string, FileInfo> fileInfo in comparisonResult.FilesToUpdate)
                    {
                        string destinationPath = Path.Combine(config.ReplicaPath, fileInfo.Key);

                        try
                        {
                            Console.WriteLine($"[INFO] Updating {fileInfo.Key} in replica folder...");
                            var creationTime = fileInfo.Value.CreationTime;
                            fileInfo.Value.CopyTo(destinationPath, true);
                            File.SetCreationTime(destinationPath, creationTime);
                            Console.WriteLine($"[SUCCESS] Updated {fileInfo.Key} in replica folder");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to update {fileInfo.Key} in replica folder: {ex.Message}");
                        }
                    }

                    foreach (KeyValuePair<string, FileInfo> fileInfo in comparisonResult.FilesToRemove)
                    {
                        try
                        {
                            Console.WriteLine($"[INFO] Deleting {fileInfo.Key} in replica folder...");
                            fileInfo.Value.Delete();
                            Console.WriteLine($"[SUCCESS] Deleted {fileInfo.Key} in replica folder");
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to remove {fileInfo.Key} in replica folder: {ex.Message}");
                        }
                    }

                    foreach (string dirPath in comparisonResult.DirectoriesToRemove)
                    {
                        try
                        {
                            Console.WriteLine($"[INFO] Deleting {dirPath} directory in replica folder...");
                            Directory.Delete(Path.Combine(config.ReplicaPath, dirPath));
                            Console.WriteLine($"[SUCCESS] Deleted {dirPath} directory in replica folder");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Failed to delete {dirPath} directory in replica folder: {ex.Message}");
                        }
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(config.SyncInterval));
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FATAL] Synchronization failed to start: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}