using Blazored.LocalStorage;
using Expensive.UI.Constants;
using System.Net.Http.Headers;

namespace Expensive.UI.Services;

public class AuthenticationHeaderHandler(ILocalStorageService storageService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization?.Scheme == AuthConstants.JwtBearerScheme)
        {
            // The request already has an Authorization header, so we don't need to add one
            return await base.SendAsync(request, cancellationToken);
        }
        var savedJwtToken = await storageService.GetItemAsync<string>(StorageConstants.AuthToken, cancellationToken);
        if (!string.IsNullOrWhiteSpace(savedJwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthConstants.JwtBearerScheme, savedJwtToken);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}