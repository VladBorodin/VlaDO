﻿using System.ComponentModel.DataAnnotations;

namespace VlaDO.DTOs
{
    public class LoginDto
    {
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
