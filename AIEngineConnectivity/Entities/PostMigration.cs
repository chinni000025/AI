namespace AIEngineConnectivity.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text;
#nullable disable
    /// <summary>
    /// This model is used for preserve post migration history.
    /// </summary>
    public class PostMigration
    {
        public int Id { get; set; }
        public string MigrationName { get; set; }
    }
}
