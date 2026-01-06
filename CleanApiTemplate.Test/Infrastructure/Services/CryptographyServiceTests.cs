using CleanApiTemplate.Infrastructure.Services;
using FluentAssertions;

namespace CleanApiTemplate.Test.Infrastructure.Services;

/// <summary>
/// Unit tests for CryptographyService
/// Demonstrates testing cryptographic operations
/// </summary>
public class CryptographyServiceTests
{
    private readonly CryptographyService _service;

    public CryptographyServiceTests()
    {
        _service = new CryptographyService();
    }

    #region Encryption Tests

    [Fact]
    public void Encrypt_ValidPlainText_ShouldReturnEncryptedString()
    {
        // Arrange
        var plainText = "This is a secret message";

        // Act
        var encrypted = _service.Encrypt(plainText);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plainText);
    }

    [Fact]
    public void Encrypt_NullPlainText_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? plainText = null;

        // Act
        Action act = () => _service.Encrypt(plainText!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Encrypt_EmptyString_ShouldThrowArgumentNullException()
    {
        // Arrange
        var plainText = string.Empty;

        // Act
        Action act = () => _service.Encrypt(plainText);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Encrypt_SamePlainTextWithDefaultKey_ShouldReturnSameEncryptedValue()
    {
        // Arrange
        var plainText = "Test Message";

        // Act
        var encrypted1 = _service.Encrypt(plainText);
        var encrypted2 = _service.Encrypt(plainText);

        // Assert
        encrypted1.Should().Be(encrypted2);
    }

    #endregion

    #region Decryption Tests

    [Fact]
    public void Decrypt_ValidEncryptedText_ShouldReturnOriginalPlainText()
    {
        // Arrange
        var plainText = "Secret data";
        var encrypted = _service.Encrypt(plainText);

        // Act
        var decrypted = _service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_NullEncryptedText_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? encryptedText = null;

        // Act
        Action act = () => _service.Decrypt(encryptedText!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Decrypt_EmptyString_ShouldThrowArgumentNullException()
    {
        // Arrange
        var encryptedText = string.Empty;

        // Act
        Action act = () => _service.Decrypt(encryptedText);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EncryptDecrypt_LongText_ShouldWorkCorrectly()
    {
        // Arrange
        var longText = new string('A', 1000);

        // Act
        var encrypted = _service.Encrypt(longText);
        var decrypted = _service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(longText);
    }

    [Fact]
    public void EncryptDecrypt_SpecialCharacters_ShouldWorkCorrectly()
    {
        // Arrange
        var specialText = "!@#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        var encrypted = _service.Encrypt(specialText);
        var decrypted = _service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(specialText);
    }

    #endregion

    #region Password Hashing Tests

    [Fact]
    public void HashPassword_ValidPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash = _service.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_NullPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? password = null;

        // Act
        Action act = () => _service.HashPassword(password!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashPassword_EmptyPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        var password = string.Empty;

        // Act
        Action act = () => _service.HashPassword(password);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HashPassword_SamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword";

        // Act
        var hash1 = _service.HashPassword(password);
        var hash2 = _service.HashPassword(password);

        // Assert - Hashes should be different because of different salts
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Password Verification Tests

    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "CorrectPassword";
        var wrongPassword = "WrongPassword";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_NullPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash = _service.HashPassword("password");

        // Act
        var result = _service.VerifyPassword(null!, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_NullHash_ShouldReturnFalse()
    {
        // Act
        var result = _service.VerifyPassword("password", null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_InvalidHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "password";
        var invalidHash = "invalid-hash";

        // Act
        var result = _service.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Token Generation Tests

    [Fact]
    public void GenerateSecureToken_DefaultLength_ShouldReturnToken()
    {
        // Act
        var token = _service.GenerateSecureToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateSecureToken_CustomLength_ShouldReturnTokenWithSpecifiedLength()
    {
        // Arrange
        var length = 64;

        // Act
        var token = _service.GenerateSecureToken(length);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var decodedBytes = Convert.FromBase64String(token);
        decodedBytes.Should().HaveCount(length);
    }

    [Fact]
    public void GenerateSecureToken_MultipleCalls_ShouldReturnDifferentTokens()
    {
        // Act
        var token1 = _service.GenerateSecureToken();
        var token2 = _service.GenerateSecureToken();
        var token3 = _service.GenerateSecureToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    #endregion

    #region SHA256 Hash Tests

    [Fact]
    public void ComputeSha256Hash_ValidInput_ShouldReturnHash()
    {
        // Arrange
        var input = "test string";

        // Act
        var hash = _service.ComputeSha256Hash(input);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveLength(64); // SHA256 produces 64 hex characters
        hash.Should().MatchRegex("^[a-f0-9]+$"); // Should be lowercase hex
    }

    [Fact]
    public void ComputeSha256Hash_NullInput_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? input = null;

        // Act
        Action act = () => _service.ComputeSha256Hash(input!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ComputeSha256Hash_EmptyString_ShouldThrowArgumentNullException()
    {
        // Arrange
        var input = string.Empty;

        // Act
        Action act = () => _service.ComputeSha256Hash(input);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ComputeSha256Hash_SameInput_ShouldReturnSameHash()
    {
        // Arrange
        var input = "consistent input";

        // Act
        var hash1 = _service.ComputeSha256Hash(input);
        var hash2 = _service.ComputeSha256Hash(input);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void ComputeSha256Hash_DifferentInputs_ShouldReturnDifferentHashes()
    {
        // Arrange
        var input1 = "input 1";
        var input2 = "input 2";

        // Act
        var hash1 = _service.ComputeSha256Hash(input1);
        var hash2 = _service.ComputeSha256Hash(input2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion
}
