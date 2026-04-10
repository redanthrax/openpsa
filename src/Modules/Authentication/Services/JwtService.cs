using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace OpenPsa.Modules.Authentication.Services;

public class JwtService : IJwtService {
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration) {
        _configuration = configuration;
    }

    public string GenerateToken(Guid userId, string email, string name, bool isSuperAdmin, IEnumerable<string> permissions) {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "openpsa";
        var audience = _configuration["Jwt:Audience"] ?? "openpsa";
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("name", name),
            new("internal_user_id", userId.ToString()),
        };

        if (isSuperAdmin)
            claims.Add(new Claim("is_super_admin", "True"));

        var permissionList = permissions.ToList();
        if (permissionList.Count > 0)
            claims.Add(new Claim("permissions", string.Join(',', permissionList)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
