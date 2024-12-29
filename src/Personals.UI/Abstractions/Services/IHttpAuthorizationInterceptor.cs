namespace Personals.UI.Abstractions.Services;

public interface IHttpAuthorizationInterceptor
{
    void RegisterEvents();
    
    void DisposeEvents();
}