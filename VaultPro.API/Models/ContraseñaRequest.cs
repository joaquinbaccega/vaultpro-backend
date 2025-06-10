using System.ComponentModel.DataAnnotations;

namespace VaultPro.API.Models;

public class ContraseñaRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public string Contraseña { get; set; } = string.Empty;
}