namespace AIEngineConnectivity.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
#nullable disable

    /// <summary>
    ///  This model is used to preserve the projects in to the Database.
    /// </summary>
    public class Project
    {
        // Primary id of the table.
        public int Id { get; set; }

        // Foreign key  who created this project
        public int CreatedByUserId { get; set; }

        // Project Name  ( not null).
        [MaxLength(100)]
        public string Name { get; set; }

        // Description of the Project.
        [MaxLength(500)]
        public string Description { get; set; }

        // Color for the UI Badge
        [MaxLength(7)]
        public string Color { get; set; } = "#6366f1";

        //Created At.
        public DateTime CreatedAt { get; set; }

        // Updated At.
        public DateTime UpdatedAt { get; set; }

        //Used for Identifying Achieved or not.
        public bool IsArchived { get; set; } = false;

        public Guid ProjectId { get; set; }

        // navigations


        // One Project --> Many Conversations.

        public ICollection<Conversation> Conversations { get; set; }


        // Many to many relations many users --> many projects.
        // not direct relations via project members.
        public ICollection<ProjectMember> ProjectMembers { get; set; }

    }
}
