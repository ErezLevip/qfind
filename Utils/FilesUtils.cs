using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace qfind.Utils
{
    public static class FilesUtils
    {
        public static void OpenFolder(string folder)
        {
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!isWindows)
            {
                var cmd = $"-c \"xdg-open {folder}\"";
                RunBash(cmd);
            }
            else
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", folder);
            }
        }
        public static void ChangeDir(string folder)
        {
            string cmd = $"-c \"cd {folder}\"";
            RunBash(cmd);
        }
        private static void RunBash(string cmd)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = cmd,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
        }
    }
}