using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using VaultPro.API.Data;

public class Requires2FAAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var email = user.Identity.Name;
        if (email == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Obtener el DbContext (desde el DI container)
        var dbContext = context.HttpContext.RequestServices.GetService(typeof(VaultDbContext)) as VaultDbContext;
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null || !usuario.Tiene2FA || !usuario.AutenticadorExpiracion.HasValue || usuario.AutenticadorExpiracion <= DateTime.UtcNow)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Autenticación 2FA requerida o expirada." });
        }
    }
}