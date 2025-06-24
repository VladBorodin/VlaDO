using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    public class UpdateRoomUserDto
    {
        [Required] public AccessLevel AccessLevel { get; set; }
    }
}
