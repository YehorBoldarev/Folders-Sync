namespace FolderSync.Source
{
    public class DirectoriesComparer
    {
        private string SourceDirectoryPath { get; }
        private string ReplicaDirectoryPath { get; }

        public DirectoriesComparer(string sourceDirectoryPath, string replicaDirectoryPath)
        {
            SourceDirectoryPath = sourceDirectoryPath;
            ReplicaDirectoryPath = replicaDirectoryPath;
        }

        public bool CheckReplicaDirectory()
            => Directory.Exists(ReplicaDirectoryPath);

        public bool CheckSourceDirectory()
            => Directory.Exists(SourceDirectoryPath);

        public (List<FileInfo>, List<FileInfo>, List<FileInfo>) Compare()
        {
            string[] sourceDirectoryFilePaths = Directory.GetFiles(SourceDirectoryPath, "*", SearchOption.AllDirectories);
            string[] replicaDirectoryFilePaths = Directory.GetFiles(ReplicaDirectoryPath, "*", SearchOption.AllDirectories);

            var sourceFilesDictionary = GetFilesDictionary(sourceDirectoryFilePaths, SourceDirectoryPath);
            var replicaFilesDictionary = GetFilesDictionary(replicaDirectoryFilePaths, ReplicaDirectoryPath);

            List<FileInfo> filesToAdd = new();
            List<FileInfo> filesToUpdate = new();
            List<FileInfo> filesToRemove = new();

            foreach (KeyValuePair<string, FileInfo> sourceKvp in sourceFilesDictionary)
            {
                if (!replicaFilesDictionary.TryGetValue(sourceKvp.Key, out var replicaValue))
                {
                    filesToAdd.Add(sourceKvp.Value);
                }
                else if (!FileComparer.Instance.Equals(sourceKvp.Value, replicaValue))
                {
                    filesToUpdate.Add(sourceKvp.Value);
                }
            }

            foreach (KeyValuePair<string, FileInfo> replicaKvp in replicaFilesDictionary)
            {
                if (!sourceFilesDictionary.ContainsKey(replicaKvp.Key))
                {
                    filesToRemove.Add(replicaKvp.Value);
                }
            }

            return (filesToAdd, filesToUpdate, filesToRemove);
        }

        private Dictionary<string, FileInfo> GetFilesDictionary(string[] filePaths, string baseDirectory)
        {
            Dictionary<string, FileInfo> result = new();

            foreach(string filePath in filePaths)
            {
                string key = Path.GetRelativePath(baseDirectory, filePath);
                FileInfo value = new(filePath);

                result[key] = value;
            }

            return result;
        }
    }
}