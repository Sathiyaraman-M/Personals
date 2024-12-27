using Microsoft.AspNetCore.Components;
using MudBlazor;
using Personals.UI.Abstractions.Services.Http;
using Personals.UI.Exceptions;
using System.Net;
using Toolbelt.Blazor;

namespace Personals.UI.Services;

public class HttpAuthorizationInterceptor(
    HttpClientInterceptor httpClientInterceptor,
    ITokenService tokenService,
    NavigationManager navigationManager,
    ISnackbar snackbar) : IDisposable
{
    private bool _disposed;

    public void RegisterEvents()
    {
        if (_disposed)
            return;
        httpClientInterceptor.BeforeSendAsync += InterceptBeforeHttpAsync;
        httpClientInterceptor.AfterSendAsync += InterceptAfterHttpAsync;
    }

    private async Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs args)
    {
        var absoluteUri = args.Request.RequestUri?.AbsoluteUri;
        if (absoluteUri == null)
            return;
        if (absoluteUri.Contains("api/token/login")
            || absoluteUri.Contains("api/token/refresh")
            || absoluteUri.Contains("/search/"))
            return;
        await RefreshTokenAsync();
    }

    private async Task InterceptAfterHttpAsync(object sender, HttpClientInterceptorEventArgs args)
    {
        var absoluteUri = args.Request.RequestUri?.AbsoluteUri;
        if (absoluteUri == null
            || absoluteUri.Contains("api/token/login")
            || absoluteUri.Contains("api/token/refresh")
            || absoluteUri.Contains("/search/"))
            return;
        if (args.Response?.StatusCode == HttpStatusCode.Unauthorized)
        {
            await RefreshTokenAsync(true);
        }
    }

    private async Task RefreshTokenAsync(bool forceRefresh = false)
    {
        try
        {
            if (forceRefresh)
            {
                await tokenService.RefreshTokenAsync();
            }
            else
            {
                await tokenService.TryRefreshTokenAsync();
            }
        }
        catch (RefreshTokenFailedException e)
        {
            Console.WriteLine(e);
            snackbar.Add("Your session has expired. Please login again.", Severity.Error);
            await tokenService.LogoutAsync();
            navigationManager.NavigateTo("/login");
        }
    }

    public void DisposeEvents()
    {
        httpClientInterceptor.BeforeSendAsync -= InterceptBeforeHttpAsync;
        httpClientInterceptor.AfterSendAsync -= InterceptAfterHttpAsync;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            DisposeEvents();
        }

        _disposed = true;
    }
}