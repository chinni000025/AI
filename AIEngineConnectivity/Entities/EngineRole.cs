using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Entities
{
#nullable disable

    /// <summary>
    /// This model is used to define the Engine Role in Db.
    /// </summary>
    public class EngineRole
    {
        public int Id { get; set; }

        public string Name { get; set; }

        //Navigation properties.
        public ICollection<Message> Messages { get; set; }

        public ICollection<GroupMessage> GroupMessages { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; }

        public ICollection<GroupChatMember> GroupChatMembers { get; set; }
    }
}
