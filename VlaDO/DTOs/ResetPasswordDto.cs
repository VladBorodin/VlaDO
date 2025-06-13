namespace VlaDO.DTOs
{
    public class ResetPasswordDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = default!;     // :contentReference[oaicite:0]{index=0}
        public string NewPassword { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
