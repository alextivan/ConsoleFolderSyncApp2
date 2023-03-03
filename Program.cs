using System.Security.Cryptography;

namespace ConsoleFolderSyncApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
           
            string sourceFolder = args[0];
            string replicaFolder = args[1];
            //int syncInterval= args[2];
            int syncInterval = int.Parse(args[2]);
            string logFilePath = args[3];
            
            if (!int.TryParse(args[2], out syncInterval))
            {
                Console.WriteLine("Error: sync_interval must be a valid integer.");
                return;
            }
            
          
            using (StreamWriter logFile = File.AppendText(logFilePath))
            {
                logFile.WriteLine($"[{DateTime.Now}] Starting synchronization...");
            }

            while (true)
            {
                try
                {
                    CopyDir(sourceFolder, replicaFolder);

                    RemoveExtraFiles(replicaFolder, sourceFolder);

                    RemoveEmptyDir(replicaFolder);

                    // Log sync
                    using (StreamWriter logFile = File.AppendText(logFilePath))
                    {
                        logFile.WriteLine($"[{DateTime.Now}] Synchronization successful");
                    }
                }
                catch (Exception ex)
                {
                    // Log failure
                    using (StreamWriter logFile = File.AppendText(logFilePath))
                    {
                        logFile.WriteLine($"[{DateTime.Now}] Synchronization failed: {ex.Message}");
                    }
                }

                // Wait sync interval
                Thread.Sleep(syncInterval);
            }
        }

        static void CopyDir(string source, string destination)
        {
            // Create dest folder
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            // Copy each file in source folder to dest folder
            foreach (string file in Directory.GetFiles(source))
            {
                string destinationFile = Path.Combine(destination, Path.GetFileName(file));

                // Copy file only if it has been modified
                if (File.Exists(destinationFile))
                {
                    byte[] sourceHash = GetMD5Hash(file);
                    byte[] destinationHash = GetMD5Hash(destinationFile);

                    if (ByteArraysEqual(sourceHash, destinationHash))
                    {
                        continue; // File has not been modified
                    }
                }

                File.Copy(file, destinationFile, true);

                // Log file copy
                Console.WriteLine($"Copied file '{file}' to '{destinationFile}'");
            }

            // Recursively copy each subdir in source folder to dest folder
            foreach (string subdirectory in Directory.GetDirectories(source))
            {
                string destinationSubdirectory = Path.Combine(destination, Path.GetFileName(subdirectory));
                CopyDir(subdirectory, destinationSubdirectory);
            }
        }

        static void RemoveExtraFiles(string directory, string otherDirectory)
        {
            // Remove files in directory that are not in otherDirectory
            foreach (string file in Directory.GetFiles(directory))
            {
                string otherFile = Path.Combine(otherDirectory, Path.GetFileName(file));

                if (!File.Exists(otherFile))
                {
                    File.Delete(file);

                    // Log file removal
                    Console.WriteLine($"Removed file '{file}'");
                }
            }

            // Recursively remove extra files in subdirectories
            foreach (string subdirectory in Directory.GetDirectories(directory))
            {
                string otherSubdirectory = Path.Combine(otherDirectory, Path.GetFileName(subdirectory));
                RemoveExtraFiles(subdirectory, otherSubdirectory);
            }
        }

        static void RemoveEmptyDir(string directory)
        {
            // Remove empty directories in directory
            foreach (string subdirectory in Directory.GetDirectories(directory))
            {
                RemoveEmptyDir(subdirectory);
                if (Directory.GetFiles(subdirectory).Length == 0 && Directory.GetDirectories(subdirectory).Length == 0)
                {
                    Directory.Delete(subdirectory);

                    // Log directory removal
                    Console.WriteLine($"Removed directory '{subdirectory}'");
                }
            }
        }

        static byte[] GetMD5Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                return md5.ComputeHash(stream);
            }
        }

        static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            else if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
        }