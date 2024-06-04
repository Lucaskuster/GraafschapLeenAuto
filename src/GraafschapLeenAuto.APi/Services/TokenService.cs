namespace GraafschapLeenAuto.Api.Services;

using GraafschapLeenAuto.Api.Context;
using GraafschapLeenAuto.Api.Entities;
using GraafschapLeenAuto.Shared.Constants;
using GraafschapLeenAuto.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

public class TokenService(IConfiguration configuration, LeenAutoDbContext dbContext)
{
    private readonly LeenAutoDbContext dbContext = dbContext;
    private readonly SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

    public string CreateToken(User user)
    {
        var userWithRoles = dbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefault(u => u.Id == user.Id);

        var claims = new List<Claim>
        {
            new Claim(Claims.Id, user.Id.ToString()),
            new Claim(Claims.Name, user.Name),
            new Claim(Claims.Email, user.Email),
            new Claim(Claims.Role, getRoles(user))
        };

        var singingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: singingCredentials,
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"]
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string getRoles(User user)
    {
        var roles = new StringBuilder();

        if (user.Roles.Any(r => r.Name == nameof(UserRole.Admin)))
        {
            roles.Append(nameof(UserRole.Admin));
        }

        if (user.Roles.Any(r => r.Name == nameof(UserRole.User)))
        {
            roles.Append(nameof(UserRole.User));
        }

        return roles.ToString();
    }
}
