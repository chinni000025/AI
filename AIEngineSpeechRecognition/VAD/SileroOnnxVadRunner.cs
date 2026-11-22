using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AIEngineSpeechRecognition.Vad;

public sealed class SileroOnnxVadRunner : IDisposable
{
    private readonly InferenceSession _session;
    private readonly int[] _stateDims;
    private DenseTensor<float> _stateTensor;
    private readonly DenseTensor<long> _srTensor;
    private readonly string _outputName;
    private readonly string? _stateOutputName;

    public SileroOnnxVadRunner(string modelPath)
    {
        var sessionOptions = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            IntraOpNumThreads = 1
        };
        _session = new InferenceSession(modelPath, sessionOptions);

        // Dynamically detect state shape (v4 is [2, 1, 64], v5 is [2, 1, 128])
        if (_session.InputMetadata.TryGetValue("state", out var stateMeta))
        {
            var d = stateMeta.Dimensions;
            _stateDims = new[]
            {
                d.Length > 0 && d[0] > 0 ? d[0] : 2,
                d.Length > 1 && d[1] > 0 ? d[1] : 1,
                d.Length > 2 && d[2] > 0 ? d[2] : 128
            };
        }
        else
        {
            _stateDims = new[] { 2, 1, 128 };
        }

        _stateTensor = new DenseTensor<float>(_stateDims);

        // Handle scalar [] or 1-D [1] for sr
        var srDims = _session.InputMetadata.TryGetValue("sr", out var srMeta) && srMeta.Dimensions.Length > 0
            ? new[] { srMeta.Dimensions[0] > 0 ? srMeta.Dimensions[0] : 1 }
            : Array.Empty<int>();
        _srTensor = new DenseTensor<long>(new long[] { 16000 }, srDims);

        // Dynamically determine output tensor names (v4 uses "output" & "stateN", v5 uses "output" & "state")
        var outputKeys = _session.OutputMetadata.Keys.ToList();
        _outputName = outputKeys.FirstOrDefault(k => k.Equals("output", StringComparison.OrdinalIgnoreCase)) ?? outputKeys[0];
        _stateOutputName = outputKeys.FirstOrDefault(k => k.Contains("state", StringComparison.OrdinalIgnoreCase));
    }

    public float Evaluate(ReadOnlyMemory<float> frame512)
    {
        try
        {
            var inputKey = _session.InputMetadata.Keys.FirstOrDefault(k => !k.Contains("state") && !k.Contains("sr")) ?? "input";
            var stateKey = _session.InputMetadata.Keys.FirstOrDefault(k => k.Contains("state")) ?? "state";
            var srKey = _session.InputMetadata.Keys.FirstOrDefault(k => k.Contains("sr")) ?? "sr";

            var inputTensor = new DenseTensor<float>(frame512.ToArray(), new[] { 1, frame512.Length });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputKey, inputTensor),
                NamedOnnxValue.CreateFromTensor(stateKey, _stateTensor),
                NamedOnnxValue.CreateFromTensor(srKey, _srTensor)
            };

            using var results = _session.Run(inputs);
            var outputTensor = results.First(r => r.Name == _outputName).AsTensor<float>();

            if (!string.IsNullOrEmpty(_stateOutputName))
            {
                var nextState = results.FirstOrDefault(r => r.Name == _stateOutputName)?.AsTensor<float>();
                if (nextState != null)
                {
                    _stateTensor = (DenseTensor<float>)nextState.ToDenseTensor();
                }
            }

            return outputTensor.GetValue(0); // Speech probability (0.0 to 1.0)
        }
        catch (Exception ex)
        {
            return 0.0f;
        }
    }

    public void Reset()
    {
        _stateTensor = new DenseTensor<float>(_stateDims);
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}
