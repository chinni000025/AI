namespace AIEngineCore.DockerServices.Windows
{
    using System.Diagnostics;
#nullable disable
    public class DockerInstallationOnWindows
    {
        public bool IsDockerAlreadyInstalled()
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "--version",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                using (Process proces = Process.Start(processStartInfo))
                {
                    if (proces == null) return false;
                    proces.WaitForExit(3000);//For respond Docker..
                    return proces.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
