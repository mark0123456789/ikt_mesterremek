using System.ComponentModel.DataAnnotations.Schema;

namespace FurniqApi.Models;

[Table("users")]
public class User
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = "";
    [Column("email")] public string Email { get; set; } = "";
    [Column("password_hash")] public string PasswordHash { get; set; } = "";
    [Column("phone")] public string? Phone { get; set; }
    [Column("role")] public string? Role { get; set; }
    [Column("registration_date")] public DateTime? RegistrationDate { get; set; }

    public List<Address> Addresses { get; set; } = [];
    public List<Cart> Carts { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}
