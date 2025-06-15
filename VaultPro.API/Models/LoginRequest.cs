namespace VaultPro.API.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Codigo2FA { get; set; }
}
