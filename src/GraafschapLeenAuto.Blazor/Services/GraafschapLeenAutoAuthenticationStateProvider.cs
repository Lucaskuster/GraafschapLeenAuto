using GraafschapLeenAuto.Shared.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GraafschapLeenAuto.Blazor.Services;

public class GraafschapLeenAutoAuthenticationStateProvider(LocalStorageService localStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetItemAsync("token");
        var principal = new ClaimsPrincipal();

        if (!string.IsNullOrEmpty(token))
        {
            principal = CreateClaimsPrincipalFromToken(token);
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));

        return new(principal);
    }

    public async Task Logout()
    {
        await localStorage.RemoveItemAsync("token");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
    }

    private static ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var identity = new ClaimsIdentity();

        if(tokenHandler.CanReadToken(token))
        {
            var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
            identity = new(
                jwtSecurityToken.Claims, 
                authenticationType: "Bearer token",
                nameType: Claims.Name,
                roleType: Claims.Role);
        }

        return new(identity);
    }
}
