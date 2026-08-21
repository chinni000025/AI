using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.Models
{
    public static class ModelCatalog
    {
        public static IReadOnlyList<ModelProvider> Providers =
        [

             new()
            {
                Name = EngineModelProviders.Groq,
                Models = new List<ModelInfo>
                {
                    new("openai/gpt-oss-20b", "GPT-OSS-20B"),
                    new("qwen/qwen3-32b", "Qwen3-32B"),
                    new("meta-llama/llama-4-scout-17b-16e-instruct", "Llama-4-Scout-17B"),
                    new("groq/compound-mini", "Compound-Mini"),
                    new("llama-3.1-8b-instant", "Llama-3.1-8B-Instant"),
                    new("llama-3.3-70b-versatile", "Llama-3.3-70B"),
                    new("allam-2-7b", "Allam-2-7B"),
                    new("openai/gpt-oss-120b", "GPT-OSS-120B")
                }
            },

            new()
            {
                Name = EngineModelProviders.Gemini,
                Models = new List<ModelInfo>
                {
                    new("gemini-2.5-flash", "Gemini-2.5-Flash"),
                    new("gemini-2.5-flash-lite", "Gemini-2.5-Flash Lite")
                }
            },


            new()
            {
                Name = EngineModelProviders.HuggingFace,
                Models = new List<ModelInfo>
                {
                    new("openai/gpt-oss-120b", "GPT-OSS"),
                    new("meta-llama/Llama-3.1-8B-Instruct", "Llama-3.1-8B"),
                    new("Qwen/Qwen2.5-7B-Instruct", "Qwen-2.5"),
                    new("deepseek-ai/DeepSeek-V3", "DeepSeek-V3")

                }
            },

            new()
            {
                Name = EngineModelProviders.OpenRouter,
                Models = new List<ModelInfo>(){
                    new("openrouter/free", "Open Router"),
                    new("nvidia/nemotron-nano-12b-v2-vl:free", "Nvidia-Nemotron-nano"),
                    new("openai/gpt-oss-20b:free", "GPT-OSS-20B")
                }
            },

            new()
            {
                Name = EngineModelProviders.Cohere,
                Models = new List<ModelInfo>(){
                    new("command-a-vision-07-2025", "CommandA Vision"),
                    new("command-a-plus-05-2026", "CommandA Plus"),
                    new("command-a-translate-08-2025", "CommandA Translate")
                }
            },

             new()
            {
                Name = EngineModelProviders.Ollama,
                Models = new List<ModelInfo>
                {
                    new("phi3", "Phi-3"),
                    new("smollm2:135m", "Smollm2-135M"),
                    new("qwen:0.5b", "Qwen-0.5B")
                }
            },
        ];
    }

    public class ModelProvider
    {
        public string Name { get; set; } = string.Empty;

        public List<ModelInfo> Models { get; set; } = new();
    }

    public record ModelInfo(
        string Value,
        string DisplayName
    );
}