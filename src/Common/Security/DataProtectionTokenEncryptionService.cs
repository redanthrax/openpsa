using Microsoft.AspNetCore.DataProtection;

namespace Common.Security;

public class DataProtectionTokenEncryptionService : ITokenEncryptionService {
    private readonly IDataProtector _protector;

    public DataProtectionTokenEncryptionService(IDataProtectionProvider provider) {
        _protector = provider.CreateProtector("OpenPsa.Tokens");
    }

    public string Encrypt(string plaintext) => _protector.Protect(plaintext);
    public string Decrypt(string ciphertext) => _protector.Unprotect(ciphertext);
}
