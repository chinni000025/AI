namespace AIEngineConnectivity.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IWhisperService
    {
        Task InitializeAsync();

        Task<string> TranscribeAudioAsync(string inputFilePath, CancellationToken cancellationToken);

    }
}
