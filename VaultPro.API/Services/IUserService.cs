namespace VaultPro.API.Services;

public interface IUserService
{
    string? Authenticate(string email, string password, string codigo2fa);
    Task<bool> ValidarCodigo2Fa(string email, string codigo2Fa);
}