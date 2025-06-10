using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaultPro.API.Models;

public class Contraseña
{
    public Guid Id { get; set; }

    [Required]
    public string Nombre { get; set; } = string.Empty; // Ej: Gmail, Facebook

    public string NombreUsuario { get; set; } = string.Empty; // <-- antes era "Usuario"

    [Required]
    public string ContraseñaCifrada { get; set; } = string.Empty;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    // FK al usuario dueño de esta contraseña
    [Required]
    public Guid UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    public Usuario Usuario { get; set; } = null!;
}