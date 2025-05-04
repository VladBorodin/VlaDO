using System.ComponentModel.DataAnnotations;
using VlaDO.Models;

namespace VlaDO.DTOs
{
    public class AddUserToRoomDto
    {
        [Required] public Guid UserId { get; set; }
        [Required] public AccessLevel AccessLevel { get; set; }
    }
}
