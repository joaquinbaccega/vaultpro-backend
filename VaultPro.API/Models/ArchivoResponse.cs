namespace VaultPro.API.Models;

public class ArchivoResponse
{
    public Guid Id { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public DateTime SubidoEn { get; set; }
}