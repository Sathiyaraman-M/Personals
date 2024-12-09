using Microsoft.AspNetCore.Components;

namespace Personals.UI.Components;

public abstract class CancellableComponent : ComponentBase, IDisposable
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed;

    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

    protected void CancelToken()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        _disposed = true;
    }
}