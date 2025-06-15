using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using VaultPro.API.Data;
using VaultPro.API.Models;
using VaultPro.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using VaultPro.API.Hubs;

namespace VaultPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArchivosController : ControllerBase
{
    private readonly VaultDbContext _context;
    private readonly ICifradoService _cifrado;
    private readonly IHubContext<VaultHub> _hub;

    public ArchivosController(VaultDbContext context, ICifradoService cifrado, IHubContext<VaultHub> hub)
    {
        _context = context;
        _cifrado = cifrado;
        _hub = hub;
    }

    private Guid? ObtenerUsuarioId()
    {
        var email = User.FindFirstValue(ClaimTypes.Name);
        return _context.Usuarios.FirstOrDefault(u => u.Email == email)?.Id;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubirArchivo([FromForm] ArchivoUploadRequest request)
    {
        var userId = ObtenerUsuarioId();
        if (userId == null) return Unauthorized();
        
        var archivoSubido = request.Archivo;
        using var ms = new MemoryStream();
        await archivoSubido.CopyToAsync(ms);
        var contenidoBytes = ms.ToArray();
        var cifrado = _cifrado.Cifrar(Convert.ToBase64String(contenidoBytes));

        var nuevo = new Archivo
        {
            Id = Guid.NewGuid(),
            NombreOriginal = archivoSubido.FileName,
            TipoMime = archivoSubido.ContentType,
            ContenidoCifrado = Encoding.UTF8.GetBytes(cifrado),
            UsuarioId = userId.Value
        };

        _context.Archivos.Add(nuevo);
        await _context.SaveChangesAsync();
        
        await _hub.Clients.All.SendAsync("archivoSubido", new
        {
            nuevo.Id,
            nuevo.NombreOriginal,
            nuevo.TipoMime,
            nuevo.SubidoEn
        });

        return Ok(new { message = "Archivo subido exitosamente" });
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var userId = ObtenerUsuarioId();
        if (userId == null) return Unauthorized();

        var archivos = await _context.Archivos
            .Where(a => a.UsuarioId == userId)
            .Select(a => new ArchivoResponse
            {
                Id = a.Id,
                NombreOriginal = a.NombreOriginal,
                TipoMime = a.TipoMime,
                SubidoEn = a.SubidoEn
            })
            .ToListAsync();

        return Ok(archivos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Descargar(Guid id)
    {
        var userId = ObtenerUsuarioId();
        if (userId == null) return Unauthorized();

        var archivo = await _context.Archivos.FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId == userId);
        if (archivo == null) return NotFound();

        var descifradoBase64 = _cifrado.Descifrar(Encoding.UTF8.GetString(archivo.ContenidoCifrado));
        var bytes = Convert.FromBase64String(descifradoBase64);

        return File(bytes, archivo.TipoMime, archivo.NombreOriginal);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var userId = ObtenerUsuarioId();
        if (userId == null) return Unauthorized();

        var archivo = await _context.Archivos.FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId == userId);
        if (archivo == null) return NotFound();

        _context.Archivos.Remove(archivo);
        await _context.SaveChangesAsync();
        
        await _hub.Clients.All.SendAsync("archivoEliminado", new { archivo.Id });
        

        return Ok(new { message = "Archivo eliminado correctamente" });
    }
    
    [HttpGet("{id}/descargar")]
    public async Task<IActionResult> DescargarArchivo(Guid id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var archivo = await _context.Archivos
            .FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId == usuarioId.Value);

        if (archivo == null) return NotFound();

        var contenidoCifrado = Encoding.UTF8.GetString(archivo.ContenidoCifrado);
        var contenidoPlanoBase64 = _cifrado.Descifrar(contenidoCifrado);
        var contenidoBytes = Convert.FromBase64String(contenidoPlanoBase64);

        return File(contenidoBytes, archivo.TipoMime, archivo.NombreOriginal);
    }

}
