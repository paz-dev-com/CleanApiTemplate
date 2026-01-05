using CleanApiTemplate.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace CleanApiTemplate.Infrastructure.Services;

/// <summary>
/// Cryptography service implementation
/// Demonstrates encryption, hashing, and secure token generation
/// WARNING: In production, use a proper encryption key management solution
/// </summary>
public class CryptographyService : ICryptographyService
{
    private readonly byte[] _defaultKey;
    private readonly byte[] _defaultIV;

    public CryptographyService()
    {
        // IMPORTANT: In production, load these from secure configuration (KeyVault)
        // This is just for demonstration purposes
        _defaultKey = Encoding.UTF8.GetBytes("YourSecure32CharacterKeyHere!!!"); // 32 bytes for AES-256
        _defaultIV = Encoding.UTF8.GetBytes("YourSecure16IV!!"); // 16 bytes
    }

    /// <summary>
    /// Encrypt using AES-256
    /// </summary>
    public string Encrypt(string plainText, string? key = null)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        var keyBytes = key != null ? Encoding.UTF8.GetBytes(key) : _defaultKey;

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = _defaultIV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypt using AES-256
    /// </summary>
    public string Decrypt(string encryptedText, string? key = null)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            throw new ArgumentNullException(nameof(encryptedText));
        }

        var keyBytes = key != null ? Encoding.UTF8.GetBytes(key) : _defaultKey;

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = _defaultIV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// Hash password using BCrypt-like algorithm (using PBKDF2)
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        // Generate a salt
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        // Hash the password with PBKDF2
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);

        // Combine salt and hash
        byte[] hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verify password against hash
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            // Extract the bytes
            byte[] hashBytes = Convert.FromBase64String(hash);

            // Extract the salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Compute the hash on the password
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hashToCompare = pbkdf2.GetBytes(32);

            // Compare the results
            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hashToCompare[i])
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generate cryptographically secure random token
    /// </summary>
    public string GenerateSecureToken(int length = 32)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Compute SHA256 hash
    /// </summary>
    public string ComputeSha256Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException(nameof(input));
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }
}
