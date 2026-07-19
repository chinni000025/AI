namespace AIEngineCore.EngineCore
{
    using System;
    using System.IO;
    using AIEngineConnectivity.Services;

    public class WindowsSystemCheck : ISystemCheckService
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
    }
}
