namespace VlaDO.DTOs
{
    public class ChangePwdDto
    {
        public string CurrentPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
