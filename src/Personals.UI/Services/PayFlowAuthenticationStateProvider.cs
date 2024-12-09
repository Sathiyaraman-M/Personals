using Blazored.LocalStorage;
using Personals.Common.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Personals.UI.Constants;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Personals.UI.Services;

public class PayFlowAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorageService) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedJwtToken = await localStorageService.GetItemAsync<string>(StorageConstants.AuthToken);
        if (string.IsNullOrWhiteSpace(savedJwtToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthConstants.JwtBearerScheme, savedJwtToken);
        var extractedClaimsFromJwt = GetClaimsFromJwt(savedJwtToken);
        var claimsIdentity = new ClaimsIdentity(extractedClaimsFromJwt, AuthConstants.JwtAuthenticationType);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return new AuthenticationState(claimsPrincipal);
    }

    public void UpdateAuthenticationState(string jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            var emptyAuthenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            NotifyAuthenticationStateChanged(Task.FromResult(emptyAuthenticationState));
            return;
        }
        var extractedClaimsFromJwt = GetClaimsFromJwt(jwtToken);
        var claimsIdentity = new ClaimsIdentity(extractedClaimsFromJwt, AuthConstants.JwtAuthenticationType);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var authenticationState = new AuthenticationState(claimsPrincipal);
        NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
    }
    
    private static List<Claim> GetClaimsFromJwt(string jwtToken)
    {
        var payload = jwtToken.Split('.')[1];
        var payloadInBytes = ParseBase64WithoutPadding(payload);
        var jwtKeyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadInBytes);

        if (jwtKeyValuePairs == null)
        {
            return [];
        }
        
        var claims = new List<Claim>();

        var permissionClaims = ExtractPermissionClaims(jwtKeyValuePairs);
        claims.AddRange(permissionClaims);

        var remainingClaims = jwtKeyValuePairs.Select(pair => new Claim(pair.Key, pair.Value.ToString()!));
        claims.AddRange(remainingClaims);
        
        return claims;
    }

    private static List<Claim> ExtractPermissionClaims(Dictionary<string, object> jwtKeyValuePairs)
    {
        jwtKeyValuePairs.TryGetValue(ApplicationClaimTypes.Permission, out var permissionClaim);
        if (permissionClaim?.ToString() == null)
        {
            return [];
        }

        var claims = new List<Claim>();
        if (permissionClaim.ToString()?.Trim().StartsWith('[') == true)
        {
            var permissionClaimValues = JsonSerializer.Deserialize<string[]>(permissionClaim.ToString()!);
            if (permissionClaimValues != null)
            {
                claims.AddRange(permissionClaimValues.Select(permission =>
                    new Claim(ApplicationClaimTypes.Permission, permission)));
            }
        }
        else
        {
            claims.Add(new Claim(ApplicationClaimTypes.Permission, permissionClaim.ToString()!));
        }
        jwtKeyValuePairs.Remove(ApplicationClaimTypes.Permission);

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}