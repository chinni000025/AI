namespace AIEngineConnectivity.Entities
{
#nullable disable
    public class ProjectMember
    {

        // Primary Id of the Table.
        public int Id { get; set; }

        //Foreign Key to the ProjectId.
        public Guid ProjectId { get; set; }

        //Foreign key for the Users Table.
        public int UserId { get; set; }

        public int RoleId { get; set; }

        public DateTime JoinedAt { get; set; }

        public Project Project { get; set; }

        public User User { get; set; }

        public EngineRole EngineRole { get; set; }
    }
}
