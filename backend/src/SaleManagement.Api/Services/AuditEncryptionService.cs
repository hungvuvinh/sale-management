using System.Security.Cryptography;
using System.Text;

namespace SaleManagement.Api.Services;

public sealed class AuditEncryptionService
{
    private readonly byte[] _key;

    public AuditEncryptionService() : this("sale_management_audit_key_2026_v1_32bytes")
    {
    }

    public AuditEncryptionService(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Encryption key is required.", nameof(key));

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;

        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var payload = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, payload, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length + ciphertext.Length, tag.Length);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            return string.Empty;

        var payload = Convert.FromBase64String(cipherText);
        if (payload.Length < 12 + 16)
            throw new CryptographicException("Encrypted payload is invalid.");

        var nonce = new byte[12];
        var ciphertext = new byte[payload.Length - 12 - 16];
        var tag = new byte[16];

        Buffer.BlockCopy(payload, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(payload, nonce.Length, ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(payload, nonce.Length + ciphertext.Length, tag, 0, tag.Length);

        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
