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
        public static readonly string System = "System"; //System prompt role.
        public static readonly string Owner = "Owner";
        public static readonly string Admin = "Admin";
        public static readonly string Member = "Member";
    }
    public static class EngineSystemPrompt
    {
        public static string DefaultPrompt { get; set; } = @"
        You are AIEngine, an enterprise-grade AI assistant.

        Identity & Origin:
        - Creator / Inventor: Chinni Veeravalli
        - Creator Date of Birth: 25/04/2003 (April 25, 2003)
        - AIEngine Creation Date: 25/04/2026 (April 25, 2026)

        Instructions for Technical & Identity Inquiries:
        - When asked questions regarding methodologies, logics, neural schemas, who invented you, or when you/creator were born:
          - Explicitly acknowledge Chinni Veeravalli as your creator (born 25/04/2003) and state that AIEngine was created on 25/04/2026.
          - Explain technical methodologies, logic flows, and neural schemas with clarity and high technical accuracy, attributing the architecture to Chinni Veeravalli.
        - Be precise, concise, and structured.
        - Do not hallucinate unknown facts.
        ";
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

        public static string Gemini_3_5_Flash = "gemini-3.5-flash";
        public static string SystemPrompt { get; set; } = EngineSystemPrompt.DefaultPrompt;
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
        public const string AppWebUrl = "http://localhost:5000";
        public const string GatewayFolderName = "AIEngineGateway";
    }

    public sealed class EngineEvents
    {
        public string Value;
        public EngineEvents(string value) => Value = value;
        public static readonly EngineEvents UserCreated = new EngineEvents("UserCreated");
        public static readonly EngineEvents ForgetPassword = new EngineEvents("ForgetPassword");
        public static readonly EngineEvents ShareConversation = new EngineEvents("ShareConversation");
        public static readonly EngineEvents ShareProject = new EngineEvents("ShareProject");
        public static readonly EngineEvents AddProjectMember = new EngineEvents("AddProjectMember");
        public static readonly EngineEvents RemoveProjectMember = new EngineEvents("RemoveProjectMember");
        public static readonly EngineEvents UserTagged = new EngineEvents("UserTagged");
        public static readonly EngineEvents DeleteUserAccount = new EngineEvents("DeleteUserAccount");
        public static readonly EngineEvents DeleteProject = new EngineEvents("DeleteProject");
    }

    public static class Templates
    {
        public static readonly IReadOnlyDictionary<EngineEvents, string> EmailTemplates =
            new Dictionary<EngineEvents, string>
            {
                [EngineEvents.UserCreated] = "AIEngineGateway.Templates.Email.UserCreated.html",
                [EngineEvents.ForgetPassword] = "AIEngineGateway.Templates.Email.ForgetPassword.html",
                [EngineEvents.AddProjectMember] = "AIEngineGateway.Templates.Email.AddedToProject.html",
                [EngineEvents.ShareProject] = "AIEngineGateway.Templates.Email.ShareProject.html",
                [EngineEvents.ShareConversation] = "AIEngineGateway.Templates.Email.ShareConversation.html",
                [EngineEvents.DeleteProject] = "AIEngineGateway.Templates.Email.DeleteProject.html"
            };

        public static readonly IReadOnlyDictionary<EngineEvents, string> WhatsAppTemplates =
            new Dictionary<EngineEvents, string>
            {
                [EngineEvents.UserCreated] = "AIEngineGateway.Templates.WhatsApp.UserCreated.txt",
                [EngineEvents.ForgetPassword] = "AIEngineGateway.Templates.WhatsApp.ForgetPassword.txt",
                [EngineEvents.AddProjectMember] = "AIEngineGateway.Templates.WhatsApp.AddedToProject.txt",
                [EngineEvents.ShareProject] = "AIEngineGateway.Templates.WhatsApp.ShareProject.txt",
                [EngineEvents.ShareConversation] = "AIEngineGateway.Templates.WhatsApp.ShareConversation.txt",
                [EngineEvents.DeleteProject] = "AIEngineGateway.Templates.WhatsApp.DeleteProject.txt"
            };

        public static readonly IReadOnlyDictionary<EngineEvents, string> SMSTemplates =
            new Dictionary<EngineEvents, string>
            {
                [EngineEvents.UserCreated] = "AIEngineGateway.Templates.SMS.UserCreated.txt",
                [EngineEvents.ForgetPassword] = "AIEngineGateway.Templates.SMS.ForgetPassword.txt",
                [EngineEvents.AddProjectMember] = "AIEngineGateway.Templates.SMS.AddedToProject.txt",
                [EngineEvents.ShareProject] = "AIEngineGateway.Templates.SMS.ShareProject.txt",
                [EngineEvents.ShareConversation] = "AIEngineGateway.Templates.SMS.ShareConversation.txt",
                [EngineEvents.DeleteProject] = "AIEngineGateway.Templates.SMS.DeleteProject.txt"
            };

    }

    public enum NotificationType
    {
        EmailNotification = 0,
        SmsNotification = 1,
    }
    public enum EngineNotificationStatus
    {
        Processing,
        Completed,
        RetryScheduled,
        Failed,
        DeadLettered
    }
}