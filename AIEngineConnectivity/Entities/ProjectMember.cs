namespace AIEngineConnectivity.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Metadata;
    using System.Text;
#nullable disable
    public class ProjectMember
    {

        // Primary Id of the Table.
        public int Id { get; set; }

        //Foreign Key to the ProjectId.
        public int ProjectId { get; set; }

        //Foreign key for the Users Table.
        public int UserId { get; set; }

        public int RoleId { get; set; }

        public DateTime JoinedAt { get; set; }

        public Project Project { get; set; }

        public User User { get; set; }

        public EngineRole EngineRole { get; set; }
    }
}
