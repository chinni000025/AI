using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IWhisperService
    {
        Task InitializeAsync();

        Task<string> TranscribeAudioAsync(string inputFilePath, CancellationToken cancellationToken);

    }
}
