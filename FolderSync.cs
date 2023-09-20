using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

class Program
{
    /*Main Method

    In the Main method, the program starts by validating command-line arguments and folder existence.
    It then enters a loop that periodically calls the SyncFolders function every 5 seconds to synchronize the folders.
    The program runs indefinitely until manually terminated.*/
    static void Main(string[] args)
    {
        /*Command-Line Arguments Handling

        The program starts by checking if it received the correct number of command-line arguments (exactly 3: sourceFolderPath, replicaFolderPath, and logFilePath).
        If the arguments are not provided correctly, it displays a usage message and exits.

        Source and Replica Folder Validation
        It checks whether the source and replica folders exist. If either of them does not exist, it displays an error message and exits.*/
        if (args.Length != 3)
        {

            return;
        }

        string sourceFolderPath = args[0];
        string replicaFolderPath = args[1];
        string logFilePath = args[2];

        if (!Directory.Exists(sourceFolderPath))
        {
            Console.WriteLine("Source folder does not exist.");
            return;
        }

        if (!Directory.Exists(replicaFolderPath))
        {
            Console.WriteLine("Replica folder does not exist.");
            return;
        }
        /*Synchronization Loop
        The program enters a loop where it repeatedly synchronizes the source folder to the replica folder every 5 seconds.*/

        Console.WriteLine($"Synchronizing {sourceFolderPath} to {replicaFolderPath} every 5 seconds...");

        while (true)
        {
            SyncFolders(sourceFolderPath, replicaFolderPath, logFilePath);
            Thread.Sleep(5000); // Synchronize every 5 seconds
        }
    }
        /*SyncFolders Function

        The SyncFolders function takes care of the actual synchronization logic.
        It first retrieves a list of all files in the source folder and its subdirectories using Directory.GetFiles with SearchOption.AllDirectories.
        For each file in the source folder:

        It computes the relative path of the file with respect to the source folder.
        It constructs the corresponding path in the replica folder.

        If the file does not exist in the replica folder or if the files are not equal (based on their MD5 hash), it performs the following steps:
        Creates any necessary subdirectories in the replica folder using Directory.CreateDirectory.

        Copies the file from the source folder to the replica folder using File.Copy.
        Logs the "Copied" operation to the log file and displays it in the console.*/
    static void SyncFolders(string sourceFolder, string replicaFolder, string logFilePath)
    {
        string[] sourceFiles = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);

        foreach (string sourceFilePath in sourceFiles)
        {
            string relativePath = sourceFilePath.Substring(sourceFolder.Length + 1);
            string replicaFilePath = Path.Combine(replicaFolder, relativePath);

            if (!File.Exists(replicaFilePath) || !AreFilesEqual(sourceFilePath, replicaFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(replicaFilePath));
                File.Copy(sourceFilePath, replicaFilePath, true);
                Log(logFilePath, $"Copied: {relativePath}");
                Console.WriteLine($"Copied: {relativePath}");
            }
        }

        // Clean up files in replica folder that do not exist in the source folder
        /*Cleaning Up Stale Files
        After copying new or modified files to the replica folder, the program goes through the files in the replica folder and its subdirectories.
        If a file exists in the replica folder that does not exist in the source folder, it deletes that file from the replica folder.
        It logs the "Deleted" operation to the log file and displays it in the console.*/

        string[] replicaFiles = Directory.GetFiles(replicaFolder, "*", SearchOption.AllDirectories);

        foreach (string replicaFilePath in replicaFiles)
        {
            string relativePath = replicaFilePath.Substring(replicaFolder.Length + 1);
            string sourceFilePath = Path.Combine(sourceFolder, relativePath);

            if (!File.Exists(sourceFilePath))
            {
                File.Delete(replicaFilePath);
                Log(logFilePath, $"Deleted: {relativePath}");
                Console.WriteLine($"Deleted: {relativePath}");
            }
        }
    }
        /*AreFilesEqual Function

        This function is used to compare the content of two files to check if they are equal.
        It computes the MD5 hash of both files and compares the hashes.
        If the hashes are equal, the files are considered equal.*/
static bool AreFilesEqual(string filePath1, string filePath2)
{
    using (var md5 = MD5.Create())
    {
        using (var stream1 = File.OpenRead(filePath1))
        using (var stream2 = File.OpenRead(filePath2))
        {
            byte[] hash1 = md5.ComputeHash(stream1);
            byte[] hash2 = md5.ComputeHash(stream2);

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return false;
                }
            }
        }
    }

    return true;
}
        /*Log Function

        The Log function is responsible for appending log messages to the specified log file.
        It includes a timestamp in the log message.*/
    static void Log(string logFilePath, string message)
    {
        string logMessage = $"{DateTime.Now}: {message}";
        File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }
}
