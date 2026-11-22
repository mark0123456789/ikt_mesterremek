namespace WarehouseApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ItemNumber { get; set; } = "";   // cikkszám
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Department { get; set; } = "";   // osztály
        public string Shelf { get; set; } = "";         // polc
        public string? SubShelf { get; set; }           // sub polc
        public int? CreatedByUserId { get; set; }
    }
}
