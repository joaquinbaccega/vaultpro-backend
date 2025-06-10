using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaultPro.API.Models;

public class Archivo
{
    public Guid Id { get; set; }

    [Required]
    public string NombreOriginal { get; set; } = string.Empty;

    [Required]
    public string TipoMime { get; set; } = string.Empty;

    [Required]
    public byte[] ContenidoCifrado { get; set; } = Array.Empty<byte>();

    public DateTime SubidoEn { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    public Usuario Usuario { get; set; } = null!;
}