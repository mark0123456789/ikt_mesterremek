namespace WarehouseApi.Models.DTOs
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string ItemNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Department { get; set; } = "";
        public string Shelf { get; set; } = "";
        public string? SubShelf { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
    }
}
