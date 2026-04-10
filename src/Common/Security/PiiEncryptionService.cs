using Microsoft.AspNetCore.DataProtection;

namespace Common.Security;

public class PiiEncryptionService : IPiiEncryptionService {
    private readonly IDataProtector _protector;

    public PiiEncryptionService(IDataProtectionProvider provider) {
        _protector = provider.CreateProtector("OpenPsa.Pii");
    }

    public string Encrypt(string plaintext) => _protector.Protect(plaintext);
    public string Decrypt(string ciphertext) => _protector.Unprotect(ciphertext);
}
