namespace WarehouseApi.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "Customer";   // Customer | Picker | Packer | Admin
    }
}
