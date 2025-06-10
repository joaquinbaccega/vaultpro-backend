using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VaultPro.API.Services;
using VaultPro.API.Data;
using Microsoft.EntityFrameworkCore;
using VaultPro.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Servicios propios
builder.Services.AddScoped<IUserService, UserService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") // o más si tenés varios
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // ← ⚠️ Esto es lo que falta
    });
});

builder.Services.AddDbContext<VaultDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("VaultProDB")));

builder.Services.AddScoped<ICifradoService, CifradoService>();

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddSignalR();
builder.Services.AddAuthorization();

// ✅ REGISTRO DE CONTROLADORES
builder.Services.AddControllers();

var app = builder.Build();

// Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<VaultHub>("/hubs/vault");
app.MapControllers();


app.Run();