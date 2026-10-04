namespace WarehouseApi.Models
{
    public class WorkTask
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";   // feladat szám
        public string Name { get; set; } = "";
        public int? PickerId { get; set; }   // kiszedő
        public int? PackerId { get; set; }   // csomagoló
        public string Status { get; set; } = "New";   // New | Picking | Picked | Packing | Done
        public DateTime CreatedAt { get; set; }        // felvétel ideje
        public DateTime? CompletedAt { get; set; }     // elkészülés ideje
    }
}
