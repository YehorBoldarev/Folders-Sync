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

        public ComparisonResult Compare()
        {
            List<string> sourceDirectoryTree = Directory.GetDirectories(SourceDirectoryPath, "*", SearchOption.AllDirectories)
                .Select(p => Path.GetRelativePath(SourceDirectoryPath, p)).ToList();
            List<string> replicaDirectoryTree = Directory.GetDirectories(ReplicaDirectoryPath, "*", SearchOption.AllDirectories)
                .Select(p => Path.GetRelativePath(ReplicaDirectoryPath, p)).ToList();

            List<string> directoriesToAdd = sourceDirectoryTree.Except(replicaDirectoryTree).ToList();
            List<string> directoriesToRemove = replicaDirectoryTree.Except(sourceDirectoryTree).ToList();

            string[] sourceDirectoryFilePaths = Directory.GetFiles(SourceDirectoryPath, "*", SearchOption.AllDirectories);
            string[] replicaDirectoryFilePaths = Directory.GetFiles(ReplicaDirectoryPath, "*", SearchOption.AllDirectories);

            var sourceFilesDictionary = GetFilesDictionary(sourceDirectoryFilePaths, SourceDirectoryPath);
            var replicaFilesDictionary = GetFilesDictionary(replicaDirectoryFilePaths, ReplicaDirectoryPath);

            Dictionary<string, FileInfo> filesToAdd = new();
            Dictionary<string, FileInfo> filesToUpdate = new();
            Dictionary<string, FileInfo> filesToRemove = new();

            foreach (KeyValuePair<string, FileInfo> sourceKvp in sourceFilesDictionary)
            {
                if (!replicaFilesDictionary.TryGetValue(sourceKvp.Key, out var replicaValue))
                {
                    filesToAdd.Add(sourceKvp.Key, sourceKvp.Value);
                }
                else if (!FileComparer.Instance.Equals(sourceKvp.Value, replicaValue))
                {
                    filesToUpdate.Add(sourceKvp.Key, sourceKvp.Value);
                }
            }

            foreach (KeyValuePair<string, FileInfo> replicaKvp in replicaFilesDictionary)
            {
                if (!sourceFilesDictionary.ContainsKey(replicaKvp.Key))
                {
                    filesToRemove.Add(replicaKvp.Key, replicaKvp.Value);
                }
            }

            return new ComparisonResult(filesToAdd, filesToUpdate, filesToRemove, directoriesToAdd, directoriesToRemove);
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

    public record ComparisonResult(
        Dictionary<string, FileInfo> FilesToAdd,
        Dictionary<string, FileInfo> FilesToUpdate,
        Dictionary<string, FileInfo> FilesToRemove,
        List<string> DirectoriesToAdd,
        List<string> DirectoriesToRemove
    );
}