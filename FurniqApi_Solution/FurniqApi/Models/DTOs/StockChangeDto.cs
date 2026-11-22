using System.ComponentModel.DataAnnotations;

namespace FurniqApi.Models.DTOs;

public class StockChangeDto
{
    /// <summary>Pozitív = beszállítás, negatív = kivét.</summary>
    public int Delta { get; set; }
}
