using System.Security.Cryptography;
using System.Text;

namespace Sertifika.Services;

/// <summary>
/// Hassas alanlari simetrik sifreler. Anahtar appsettings "Security:EncryptionKey" (base64, 32 byte).
/// Plain metinler "enc:" prefix'i olmadan, sifrelenenler "enc:&lt;base64&gt;" ile saklanir.
/// Boylece eski (plain) kayitlar kirmadan yan yana yasar; dokunuldukca sifrelenir.
/// Format: enc:&lt;base64(nonce[12] || ciphertext || tag[16])&gt;
/// </summary>
public class EncryptionService
{
    private const string Prefix = "enc:";
    private readonly byte[] _key;

    public EncryptionService(IConfiguration config)
    {
        var b64 = config["Security:EncryptionKey"];
        if (string.IsNullOrWhiteSpace(b64))
            throw new InvalidOperationException(
                "Security:EncryptionKey appsettings'te ayarlanmamis. 32 byte'lik base64 uretin: " +
                "powershell> [Convert]::ToBase64String((1..32 | %{Get-Random -Max 256}))");

        _key = Convert.FromBase64String(b64);
        if (_key.Length != 32)
            throw new InvalidOperationException("Security:EncryptionKey 32 byte olmali (AES-256).");
    }

    public string Encrypt(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return string.Empty;
        if (plaintext.StartsWith(Prefix)) return plaintext; // cift sifreleme engeli

        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        var cipher = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var output = new byte[nonce.Length + cipher.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, nonce.Length);
        Buffer.BlockCopy(cipher, 0, output, nonce.Length, cipher.Length);
        Buffer.BlockCopy(tag, 0, output, nonce.Length + cipher.Length, tag.Length);

        return Prefix + Convert.ToBase64String(output);
    }

    public string Decrypt(string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return string.Empty;
        if (!stored.StartsWith(Prefix)) return stored; // eski plain veriler olduklari gibi dondurulur

        try
        {
            var data = Convert.FromBase64String(stored.Substring(Prefix.Length));
            if (data.Length < 12 + 16) return string.Empty;

            var nonce = new byte[12];
            var tag = new byte[16];
            var cipher = new byte[data.Length - 12 - 16];
            Buffer.BlockCopy(data, 0, nonce, 0, 12);
            Buffer.BlockCopy(data, 12, cipher, 0, cipher.Length);
            Buffer.BlockCopy(data, 12 + cipher.Length, tag, 0, 16);

            var plain = new byte[cipher.Length];
            using var aes = new AesGcm(_key, tag.Length);
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return string.Empty;
        }
    }
}
