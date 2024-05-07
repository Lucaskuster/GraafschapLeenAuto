using GraafschapLeenAuto.Api.Context;
using GraafschapLeenAuto.Shared;

namespace GraafschapLeenAuto.Api.Services;

public class AuthService(LeenAutoDbContext dbContext, TokenService tokenService)
{
    public AuthResponse? Login(LoginRequest request)
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null || user.Password != request.Password)
        {
            return null;
        }

        return new AuthResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = tokenService.CreateToken(user),
        };
    }
}
