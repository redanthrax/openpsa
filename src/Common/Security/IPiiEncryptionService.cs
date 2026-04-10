namespace Common.Security;

public interface IPiiEncryptionService {
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
