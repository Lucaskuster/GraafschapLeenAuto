namespace GraafschapLeenAuto.Blazor.Handler;

using GraafschapLeenAuto.Blazor.Services;
using System.Net.Http.Headers;

public class AuthorizationMessageHandler(LocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var bearerToken = await localStorage.GetItemAsync("token");

        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        } else
        {
            throw new ArgumentNullException(nameof(request), "No token found in local storage");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
