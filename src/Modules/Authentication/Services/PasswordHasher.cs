using System.Security.Cryptography;
using System.Text;

namespace OpenPsa.Modules.Authentication.Services;

public static class PasswordHasher {
    public static string Hash(string password) {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        var result = new byte[48];
        salt.CopyTo(result, 0);
        hash.CopyTo(result, 16);
        return Convert.ToBase64String(result);
    }

    public static bool Verify(string password, string hash) {
        var bytes = Convert.FromBase64String(hash);
        var salt = bytes[..16];
        var stored = bytes[16..];
        var computed = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(computed, stored);
    }
}
