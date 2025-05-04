using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    public class GenerateTokenDto
    {
        [Required] public AccessLevel AccessLevel { get; set; }
        [Range(1, 30)] public int DaysValid { get; set; }
    }
}
