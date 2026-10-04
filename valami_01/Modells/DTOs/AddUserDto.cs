namespace WarehouseApi.Models.DTOs
{
    public class AddUserDto
    {
        public string Name { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "Customer";
    }
}
