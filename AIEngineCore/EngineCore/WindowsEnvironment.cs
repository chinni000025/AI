namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.Services;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
#nullable disable
    public class WindowsEnvironment : IEnvironment
    {
        private double GetSystemRamDetails()
        {
            long bytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            return (bytes / Math.Pow(1024, 3));
        }

        private double GetAvailableFreeSpace()
        {
            DriveInfo cDrive = new DriveInfo("C");
            return (cDrive.TotalFreeSpace / Math.Pow(1024, 3));
        }

        public bool CanInstallEngine()
        {
            return GetSystemRamDetails() >= 7.5 && GetAvailableFreeSpace() > 10;
        }

        public async Task installDocker()
        {
            string installerPath = Path.Combine(Path.GetTempPath(), "DockerDesktopInstaller.exe");

            bool downloadSuccess = await DownloadInstallerAsync(installerPath);
            if (!downloadSuccess)
            {
                throw new Exception("Failed to Download Docker");
            }

            bool installSuccess = ExecuteInstaller(installerPath);
            if (!installSuccess)
            {
                throw new Exception("Failed to Install Docker");
            }
        }
        private async Task<bool> DownloadInstallerAsync(string installerPath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

                    using (HttpResponseMessage response = await client.GetAsync(InstallerConstants.DockerInstallURL, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                        using (Stream streamToWriteTo = File.Open(installerPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool ExecuteInstaller(string installerPath)
        {
            try
            {
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "install --quiet --accept-license",
                    UseShellExecute = true,
                    Verb = "runas"
                }))
                {
                    if (process == null) return false;

                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (File.Exists(installerPath))
                {
                    try { File.Delete(installerPath); } catch { }
                }
            }
        }

        public bool isDockerAvailable()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(startInfo);
                if (process is null) return false;
                process.WaitForExit(3000);//3 seconds.
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
