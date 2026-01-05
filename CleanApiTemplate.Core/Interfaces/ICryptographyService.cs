namespace CleanApiTemplate.Core.Interfaces;

/// <summary>
/// Interface for cryptographic operations
/// Provides encryption, decryption, and hashing capabilities
/// </summary>
public interface ICryptographyService
{
    /// <summary>
    /// Encrypt a string value using AES encryption
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <param name="key">Encryption key (optional, uses default if not provided)</param>
    /// <returns>Encrypted text in Base64 format</returns>
    string Encrypt(string plainText, string? key = null);

    /// <summary>
    /// Decrypt an encrypted string value
    /// </summary>
    /// <param name="encryptedText">Encrypted text in Base64 format</param>
    /// <param name="key">Decryption key (optional, uses default if not provided)</param>
    /// <returns>Decrypted plain text</returns>
    string Decrypt(string encryptedText, string? key = null);

    /// <summary>
    /// Create a hash of a password using bcrypt or similar secure algorithm
    /// </summary>
    /// <param name="password">Password to hash</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    /// <param name="password">Password to verify</param>
    /// <param name="hash">Hash to verify against</param>
    /// <returns>True if password matches hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Generate a cryptographically secure random token
    /// </summary>
    /// <param name="length">Length of the token</param>
    /// <returns>Random token as Base64 string</returns>
    string GenerateSecureToken(int length = 32);

    /// <summary>
    /// Create a SHA256 hash of a string
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>SHA256 hash in hexadecimal format</returns>
    string ComputeSha256Hash(string input);
}