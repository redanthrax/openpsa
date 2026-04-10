using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Security;

public class EncryptedStringConverter : ValueConverter<string?, string?> {
    public EncryptedStringConverter(IPiiEncryptionService encryption)
        : base(
            v => v == null ? null : encryption.Encrypt(v),
            v => v == null ? null : encryption.Decrypt(v)) {
    }
}
