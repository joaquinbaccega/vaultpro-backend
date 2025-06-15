using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AESHelper
{
    private static readonly string Key = "TU_CLAVE_AES_DE_32_BYTES_EXACTOS!"; // 32 chars = 256 bits
    private static readonly string IV = "TU_VECTOR_INICIAL_16B"; // 16 chars

    public static string Desencriptar(string textoCifrado)
    {
        var buffer = Convert.FromBase64String(textoCifrado);
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Key);
        aes.IV = Encoding.UTF8.GetBytes(IV);
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
        return reader.ReadToEnd();
    }

    // (opcional) Método para cifrar
    public static string Encriptar(string textoPlano)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Key);
        aes.IV = Encoding.UTF8.GetBytes(IV);
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cs);
        writer.Write(textoPlano);
        writer.Flush();
        cs.FlushFinalBlock();
        return Convert.ToBase64String(ms.ToArray());
    }
}