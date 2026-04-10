namespace OpenPsa.Modules.Authentication.Services;

public interface IJwtService {
    string GenerateToken(Guid userId, string email, string name, bool isSuperAdmin, IEnumerable<string> permissions);
}
