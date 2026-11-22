using System.ComponentModel.DataAnnotations.Schema;

namespace FurniqApi.Models;

[Table("addresses")]
public class Address
{
    [Column("id")] public int Id { get; set; }
    [Column("user_id")] public int UserId { get; set; }
    [Column("name")] public string Name { get; set; } = "";
    [Column("postal_code")] public string PostalCode { get; set; } = "";
    [Column("city")] public string City { get; set; } = "";
    [Column("street")] public string Street { get; set; } = "";
    [Column("house_number")] public string HouseNumber { get; set; } = "";
    [Column("is_default")] public bool? IsDefault { get; set; }

    public User? User { get; set; }
}
