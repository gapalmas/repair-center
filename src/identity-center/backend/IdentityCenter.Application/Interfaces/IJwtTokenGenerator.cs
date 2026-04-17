using IdentityCenter.Domain.Entities;

namespace IdentityCenter.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(Usuario usuario);
    string GenerateRefreshToken();
}
