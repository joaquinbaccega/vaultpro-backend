using System.Security.Cryptography;
using System.Text;

namespace VaultPro.API.Services;

public interface ICifradoService
{
    string Cifrar(string textoPlano);
    string Descifrar(string textoCifrado);
}

public class CifradoService : ICifradoService
{
    private readonly string _clave;

    public CifradoService(IConfiguration config)
    {
        _clave = config["Crypto:Key"] ?? throw new Exception("Falta la clave de cifrado en appsettings.json");
    }

    public string Cifrar(string textoPlano)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_clave);
        aes.GenerateIV();
        var iv = aes.IV;

        using var encryptor = aes.CreateEncryptor();
        var datos = Encoding.UTF8.GetBytes(textoPlano);
        var cifrado = encryptor.TransformFinalBlock(datos, 0, datos.Length);

        var resultado = new byte[iv.Length + cifrado.Length];
        Buffer.BlockCopy(iv, 0, resultado, 0, iv.Length);
        Buffer.BlockCopy(cifrado, 0, resultado, iv.Length, cifrado.Length);

        return Convert.ToBase64String(resultado);
    }

    public string Descifrar(string textoCifrado)
    {
        var datos = Convert.FromBase64String(textoCifrado);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_clave);
        var iv = new byte[16];
        var cifrado = new byte[datos.Length - 16];
        Buffer.BlockCopy(datos, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(datos, iv.Length, cifrado, 0, cifrado.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var resultado = decryptor.TransformFinalBlock(cifrado, 0, cifrado.Length);

        return Encoding.UTF8.GetString(resultado);
    }
}