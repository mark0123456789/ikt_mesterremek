namespace WarehouseApi.Models
{
    public class BacklogEntry
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = "";   // Pick | Pack
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
