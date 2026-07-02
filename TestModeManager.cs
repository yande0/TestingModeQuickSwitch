using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SwitchTestingMode
{
    public static class TestModeManager
    {
        static string GetBcdeditPath()
        {
            string systemDir = Environment.SystemDirectory;
            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                systemDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysNative");
            }
            return Path.Combine(systemDir, "bcdedit.exe");
        }

        public static bool IsTestModeEnabled()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = GetBcdeditPath(),
                    Arguments = "/enum {current}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(5000);

                    var match = Regex.Match(output, @"testsigning\s+(Yes|No)", RegexOptions.IgnoreCase);
                    return match.Success && match.Groups[1].Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetTestMode(bool enable)
        {
            var psi = new ProcessStartInfo
            {
                    FileName = GetBcdeditPath(),
                    Arguments = enable ? "/set testsigning on" : "/set testsigning off",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
            {
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit(15000);

                if (p.ExitCode != 0)
                    throw new InvalidOperationException($"bcdedit 执行失败: {error.Trim()}");
            }
        }

        public static void RestartSystem()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown",
                Arguments = "/r /t 0",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        public static string GetWindowsVersion()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string name = key.GetValue("ProductName", "Windows") as string;
                        string ver = key.GetValue("DisplayVersion", "") as string;
                        string build = key.GetValue("CurrentBuild", "") as string;

                        if (!string.IsNullOrEmpty(ver))
                            return $"{name} {ver} (Build {build})";
                        return $"{name} (Build {build})";
                    }
                }
            }
            catch { }

            return Environment.OSVersion.VersionString;
        }
    }
}
