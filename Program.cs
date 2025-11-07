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
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}