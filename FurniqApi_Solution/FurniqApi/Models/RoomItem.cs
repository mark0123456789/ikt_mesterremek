using System.ComponentModel.DataAnnotations.Schema;

namespace FurniqApi.Models;

public abstract class RoomItem
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } = "";
    [Column("description")] public string? Description { get; set; }
    [Column("price", TypeName = "decimal(10,2)")] public decimal Price { get; set; }
    [Column("stock")] public int Stock { get; set; }
    [Column("image_url")] public string? ImageUrl { get; set; }
    [Column("active")] public bool? Active { get; set; }
}
// ---- Szobánkénti terméktáblák (azonos szerkezetű táblák) ----
// Az alaposztály nincs a modellben entitásként, így a tulajdonságai a leszármazottakba kerülnek.
[Table("bathroom")] public class Bathroom : RoomItem { }
[Table("bedroom")] public class Bedroom : RoomItem { }
[Table("hallway")] public class Hallway : RoomItem { }
[Table("kitchen")] public class Kitchen : RoomItem { }
[Table("livingroom")] public class Livingroom : RoomItem { }
