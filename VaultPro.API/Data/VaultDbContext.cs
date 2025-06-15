using Microsoft.EntityFrameworkCore;
using VaultPro.API.Models;

namespace VaultPro.API.Data;

public class VaultDbContext : DbContext
{
    public VaultDbContext(DbContextOptions<VaultDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Contraseña> Contraseñas { get; set; }
    public DbSet<Archivo> Archivos { get; set; }

}