using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VaultPro.API.Data;
using VaultPro.API.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using OtpNet;

namespace VaultPro.API.Services;

public class UserService : IUserService
{
    private readonly IConfiguration _config;
    private readonly VaultDbContext _context;

    public UserService(IConfiguration config, VaultDbContext context)
    {
        _config = config;
        _context = context;
    }

    public string? Authenticate(string email, string password, string? codigo2fa = null)
    {
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);
        if (usuario == null) return null;

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
        if (!isPasswordValid) return null;

        // Validar 2FA si está habilitado
        if (usuario.Tiene2FA)
        {
            Console.WriteLine($"Código ingresado: {codigo2fa}");
            if (string.IsNullOrWhiteSpace(codigo2fa)) return "REQUIERE_2FA";

            var totp = new Totp(Base32Encoding.ToBytes(usuario.Secreto2FA!));
            Console.WriteLine($"Código esperado: {totp.ComputeTotp()}");
            if (!totp.VerifyTotp(codigo2fa, out _, new VerificationWindow(1, 1)))
                return null;
        }

        // Generar token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
}