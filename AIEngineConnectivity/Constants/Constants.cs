namespace AIEngineConnectivity.Constants
{
    public static class EngineConstants
    {
        public static readonly string ForceLogout = "ForceLogout";
        public static readonly string EngineResponse = "EngineResponse";
        public static readonly string EngineLogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIEngine/AIEngineLogs");
        public static readonly string EngineLogPath = Path.Combine(EngineLogDirectory, "Log-.txt");
        public static readonly string EngineStateChanged = "EngineStateChange";

        public enum DataBaseProvider
        {
            SqlServer = 1,
            PostgreSql = 2,
        }
    }

    public static class AuthConstants
    {
        public static readonly string EngineIgnition = "EngineIgnition"; // Access Token
        public static readonly string EngineRestart = "EngineRestart"; //Refresh Token
        public static readonly string EngineValidationToken = "EngineValidationToken"; // Antiforgery Key.
        public static readonly string EnginesVerification = "EngineVerification";
    }


    public static class Permissions
    {
        public static readonly string Read = "Read"; // Able to read the chat.
        public static readonly string Write = "Write"; // Able to write the chat.
    }

    public static class EngineRoles
    {
        public static readonly string User = "User"; // User conversation
        public static readonly string Assistant = "Assistant"; // Assistant Conversation.
        public static readonly string Owner = "Owner";
        public static readonly string Admin = "Admin";
        public static readonly string Member = "Member";
    }

    public static class EngineModels
    {
        //Ollama Models
        public const string Phi3 = "phi3";
        public const string Smollm2 = "smollm2:135m";
        public const string Qwen0_5b = "qwen:0.5b";

        // Gemini Models
        public const string Gemini_2_5_Flash = "gemini-2.5-flash";
        public const string Gemini_2_5_Flash_Lite = "gemini-2.5-flash-lite";

        // Groq Models
        public const string Groq_GPT_OSS_20B = "openai/gpt-oss-20b";
        public const string Groq_Qwen3_32B = "qwen/qwen3-32b";
        public const string Groq_Llama_4_Scout_17B = "meta-llama/llama-4-scout-17b-16e-instruct";
        public const string Groq_Compound_Mini = "groq/compound-mini";
        public const string Groq_Llama_3_1_8B_Instant = "llama-3.1-8b-instant";
        public const string Groq_Llama_3_3_70B = "llama-3.3-70b-versatile";
        public const string Groq_Allam_2_7B = "allam-2-7b";
        public const string Groq_GPT_OSS_120B = "openai/gpt-oss-120b";

        //HuggingFace Models
        public const string HF_GPT_2 = "gpt2";
        public const string HF_Llama_8b = "meta-llama/llama-3.1-8b-instruct";
        public const string HF_Qwen = "qwen/webworld-8b:featherless-ai";
        public const string HF_google_gemma = "google/gemma-4-31b-it";
        public const string HF_deepseek_coding = "deepseek-ai/deepseek-v4-pro";

        //Cohere Models
        public const string Cohere_CommandA_Vision = "command-a-vision-07-2025";
        public const string Cohere_CommandA_Plus = "command-a-plus-05-2026";
        public const string Cohere_CommandA_Translate = "command-a-translate-08-2025";
    }

    public static class EngineModelProviders
    {
        public const string Ollama = "ollama";
        public const string Gemini = "gemini";
        public const string Groq = "groq";
        public const string HuggingFace = "huggingface";
        public const string OpenRouter = "openrouter";
        public const string Cohere = "cohere";
    }

    public static class ConversationUpdatingPaths
    {
        public const string Title = "/Title";
        public const string IsArchived = "/IsArchived";
        public const string IsFavorite = "/IsFavorite";
        public const string IsPinned = "/IsPinned";
        public const string ModelUsed = "/ModelUsed";
    }

    public static class ModelUrl
    {
        public static readonly string OllamaUrl = "http://localhost:11434";
        public static readonly string GroqUrl = "https://api.groq.com/openai/v1/";
        public static readonly string HfUrl = "https://router.huggingface.co/v1/";
        public static readonly string ORUrl = "https://openrouter.ai/api/v1/";
        public static readonly string CohereUrl = "https://api.cohere.com/v2/";
    }

    public static class GeminiConfigurations
    {
        public static double Temperature { get; set; } = 0.3;
        public static int MaxTokens { get; set; } = 512;

        public static string Gemini_2_5_Flash = "gemini-2.5-flash";
        public static string SystemPrompt { get; set; } = @"
            You are an enterprise-grade AI assistant.

            Rules:
            - Be precise and concise
            - Do not hallucinate unknown facts
            - Prefer structured responses when possible
            - Avoid unnecessary explanations
            - Optimize for clarity and correctness

            Output format:
            - Use bullet points when applicable
            - Keep responses under control unless explicitly asked
            ";
    }

    public static class WhisperConstants
    {
        public const string WhisperModel = "ggml-base.en.bin";
        public const string ModelsFolder = "Models";
        public const string AudioConverter = "FFmpeg";
        public const string AudioExecutionFile = "ffmpeg.exe";
        public const string Language = "en";
        public const string SamplingRate = "-ar 16000"; // sampling rate 16khz..
        public const string Channel = "-ac 1";
        public const string AudioCode = "-c:a pcm_s16le"; //uses pluse code modulation 16 bit samples with little ending.
    }

    public static class Connection
    {
        public const string Google = "Google";
        public const string Smtp = "Smtp";
    }

    public static class GoogleConnectionConstants
    {
        public const string ClientId = "client_id";
        public const string ClientSecret = "client_secret";
        public const string AuthCode = "code";
        public const string GrantType = "grant_type";
        public const string RedirectUri = "redirect_uri";
        public const string OAuthTokenEndPoint = "https://oauth2.googleapis.com/token";
    }
    public static class EngineEncyrption
    {
        public static readonly string RSAEncryption = "RSA";
    }

    public enum Platform
    {
        Window = 1,
        Linux = 2
    }

    public static class InstallerConstants
    {
        public const string DockerInstallURL = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe";
    }
}