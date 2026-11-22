
namespace AIEngineGateway.EngineInfrastructure.DatabaseScripts
{
    public static class ScriptLoader
    {
        public static async Task<string> LoadScriptsAsync(string scriptName)
        {
            var assembly = typeof(ScriptLoader).Assembly;
            var assemblyName = assembly.GetName().Name;
            var resourceName = $"{assemblyName}.EngineInfrastructure.DatabaseScripts.{scriptName}";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
                throw new FileNotFoundException($"Embedded SQL resource '{resourceName}' was not found in assembly '{assembly.FullName}'.");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
