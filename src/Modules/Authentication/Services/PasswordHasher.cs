using System.Security.Cryptography;

namespace OpenPsa.Modules.Authentication.Services;

public static class PasswordHasher {
    public static string Hash(string password) {
        var salt = RandomNumberGenerator.GetBytes(16);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        var result = new byte[48];
        salt.CopyTo(result, 0);
        hash.CopyTo(result, 16);
        return Convert.ToBase64String(result);
    }

    public static bool Verify(string password, string hash) {
        var bytes = Convert.FromBase64String(hash);
        var salt = bytes[..16];
        var stored = bytes[16..];
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var computed = pbkdf2.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(computed, stored);
    }
}
