namespace AIEngineConnectivity.Services
{
    public interface IEnvironment
    {
        bool CanInstallEngine();
        public Task installDocker();
    }
}
