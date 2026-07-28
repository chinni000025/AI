namespace AIEngineConnectivity.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public class ProjectDTO
    {
        public string ProjectName { get; set; }
        public List<ProjectConversation> Conversations { get; set; }
    }
    public class ProjectConversation
    {
        public string Title { get; set; }
        public Guid ConversationId { get; set; }
    }
}
