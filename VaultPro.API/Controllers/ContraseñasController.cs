using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VaultPro.API.Data;
using VaultPro.API.Models;
using VaultPro.API.Services;
using Microsoft.EntityFrameworkCore;

namespace VaultPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContraseñasController : ControllerBase
{
    private readonly VaultDbContext _context;
    private readonly ICifradoService _cifrado;

    public ContraseñasController(VaultDbContext context, ICifradoService cifrado)
    {
        _context = context;
        _cifrado = cifrado;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ContraseñaRequest request)
    {
        var userId = ObtenerUsuarioId();
        if (userId == null) return Unauthorized();

        var contraseña = new Contraseña
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            NombreUsuario = request.NombreUsuario,
            ContraseñaCifrada = _cifrado.Cifrar(request.Contraseña),
            UsuarioId = userId.Value
        };

        _context.Contraseñas.Add(contraseña);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Contraseña guardada" });
    }

    [HttpGet]
    public async Task<IActionResult> Obtener()
    {
        var userId = ObtenerUsuarioId();
        if (userId == null) return Unauthorized();

        var contraseñas = await _context.Contraseñas
            .Where(c => c.UsuarioId == userId)
            .Select(c => new
            {
                c.Id,
                c.Nombre,
                c.NombreUsuario,
                Contraseña = _cifrado.Descifrar(c.ContraseñaCifrada),
                c.CreadoEn
            })
            .ToListAsync();

        return Ok(contraseñas);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var userId = ObtenerUsuarioId();
        if (userId == null) return Unauthorized();

        var contraseña = await _context.Contraseñas
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == userId);

        if (contraseña == null)
            return NotFound();

        _context.Contraseñas.Remove(contraseña);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Contraseña eliminada" });
    }

    private Guid? ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.Name);
        if (claim == null) return null;

        var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == claim.Value);
        return usuario?.Id;
    }
}
