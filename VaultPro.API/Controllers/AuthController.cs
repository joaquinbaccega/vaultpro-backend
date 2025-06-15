using Microsoft.AspNetCore.Mvc;
using VaultPro.API.Models;
using VaultPro.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VaultPro.API.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using System.Web;

namespace VaultPro.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly VaultDbContext _context;

    public AuthController(IUserService userService, VaultDbContext context)
    {
        _userService = userService;
        _context = context;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _userService.Authenticate(request.Email, request.Password, request.Codigo2FA);

        if (result == "REQUIERE_2FA")
            return BadRequest(new { require2fa = true });

        if (result == null)
            return Unauthorized(new { message = "Credenciales inválidas o código 2FA incorrecto" });

        return Ok(new { token = result });
    }


    [Authorize]
    [HttpGet("perfil")]
    public IActionResult Perfil()
    {
        var email = User.Identity?.Name;
        var rol = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Email = email,
            Rol = rol
        });
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validar si el usuario ya existe
        var usuarioExistente = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email);

        if (usuarioExistente)
            return Conflict(new { message = "Ya existe un usuario con ese email" });

        // Hashear la contraseña
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var nuevoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHash,
            Tiene2FA = false,
            Secreto2FA = null,
            CreadoEn = DateTime.UtcNow
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Usuario registrado correctamente" });
    }
    
    [Authorize]
    [HttpPost("activar-2fa")]
    public async Task<IActionResult> Activar2FA()
    {
        var email = User.Identity?.Name;
        if (email == null) return Unauthorized();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        if (usuario == null) return Unauthorized();

        // Generar secreto TOTP
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);

        // Guardar secreto en la DB
        usuario.Secreto2FA = base32Secret;
        usuario.Tiene2FA = true;
        await _context.SaveChangesAsync();

        // Crear URL otpauth://...
        var label = HttpUtility.UrlEncode($"VaultPro:{usuario.Email}");
        var issuer = HttpUtility.UrlEncode("VaultPro");
        var otpUrl = $"otpauth://totp/{label}?secret={base32Secret}&issuer={issuer}";

        // Convertir a código QR base64
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(otpUrl, QRCoder.QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCoder.Base64QRCode(qrData);
        var qrBase64 = qrCode.GetGraphic(10);

        return Ok(new
        {
            secret = base32Secret,
            qrCodeBase64 = $"data:image/png;base64,{qrBase64}"
        });
    }
}