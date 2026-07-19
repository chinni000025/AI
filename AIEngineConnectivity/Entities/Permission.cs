namespace AIEngineConnectivity.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text;
#nullable disable

    /// <summary>
    /// This Model is used for Permissions to the AIEngine
    /// </summary>
    public class Permission
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<ConversationShare> ConversationShares { get; set; }
    }
}
