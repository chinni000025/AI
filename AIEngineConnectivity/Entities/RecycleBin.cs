using AIEngineConnectivity.Constants;

namespace AIEngineConnectivity.Entities
{
    public class RecycleBin
    {
        public Guid Id { get; set; }
        public RecycleItem ItemId { get; set; }
        public string EntityId { get; set; }
        public string DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}