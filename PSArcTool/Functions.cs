using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace PSArcTool
{
    [SuppressMessage("ReSharper", "UseStringInterpolation")]
    public static class Functions
    {
        public static void Run(string arguments, string workingDirectory)
        {
            var path = Path.GetTempFileName() + ".exe"; // Actually PSARC.EXE, but we just call it something random just cause.
            File.WriteAllBytes(path, Properties.Resources.psarc);
            var procInfo = new ProcessStartInfo(path, arguments);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                procInfo.WorkingDirectory = workingDirectory;
            }
            Process.Start(procInfo);
        }

        public static void Extract(string pakFilePath)
        {
            var pakFile = new FileInfo(pakFilePath);
            if (pakFile.Exists)
            {
                var psarcArgs = string.Format("extract -y \"{0}\"", pakFile.FullName);
                Run(psarcArgs, pakFile.DirectoryName);
            }
        }

        public static void Create(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                Create(GetFileList("*", dirPath).ToList(), Directory.GetParent(dirPath).FullName);
            }
        }

        public static void Create(IList<string> paths, string rootPath = null)
        {
            if (paths.Any() && paths.All(path => File.Exists(path) || Directory.Exists(path)))
            {
                // If no rootPath provided, we assume first file is the top-most relative to other files.
                if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                {
                    var firstPath = paths.First();
                    if (File.Exists(firstPath))
                    {
                        rootPath = new FileInfo(firstPath).DirectoryName;
                    }
                    else if (Directory.Exists(firstPath))
                    {
                        rootPath = Directory.GetParent(firstPath).FullName;
                    }
                }

                var filePaths = paths.Where(File.Exists).ToList();

                foreach (var dirPath in paths.Where(Directory.Exists))
                {
                    filePaths.AddRange(GetFileList("*", dirPath));
                }

                if (!string.IsNullOrEmpty(rootPath) && Directory.Exists(rootPath))
                {
                    var tmpFilePath = Path.GetTempFileName();
                    using (var writer = new StreamWriter(tmpFilePath))
                    {
                        foreach (var filePath in filePaths)
                        {
                            writer.WriteLine(filePath);
                        }
                    }
                    Run(
                        string.Format("create -a --zlib --inputfile=\"{0}\" --output={1}", tmpFilePath, "psarc.pak"),
                        rootPath);
                }
            }
        }

        public static
            IEnumerable<string> GetFileList(string fileSearchPattern, string rootFolderPath)
        {
            var pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                var tmp = Directory.GetFiles(rootFolderPath, fileSearchPattern);
                foreach (var t in tmp)
                {
                    yield return t;
                }
                tmp = Directory.GetDirectories(rootFolderPath);
                foreach (var t in tmp)
                {
                    pending.Enqueue(t);
                }
            }
        }

    }
}
