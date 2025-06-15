using System.ComponentModel.DataAnnotations;

namespace VaultPro.API.Models;

public class Usuario
{
    public Guid Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public string Rol { get; set; } = "User";

    public bool Tiene2FA { get; set; } = false;

    public string? Secreto2FA { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    
    public DateTime? AutenticadorUltimoUso { get; set; } = DateTime.UtcNow;
    public DateTime? AutenticadorExpiracion { get; set; } = DateTime.UtcNow.AddMinutes(5);
}