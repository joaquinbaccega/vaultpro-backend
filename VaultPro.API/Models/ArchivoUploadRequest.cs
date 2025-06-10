using Microsoft.AspNetCore.Http;

namespace VaultPro.API.Models;

public class ArchivoUploadRequest
{
    public IFormFile Archivo { get; set; } = null!;
}