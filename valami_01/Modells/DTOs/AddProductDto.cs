namespace WarehouseApi.Models.DTOs
{
    public class AddProductDto
    {
        public string ItemNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Department { get; set; } = "";
        public string Shelf { get; set; } = "";
        public string? SubShelf { get; set; }
    }
}
