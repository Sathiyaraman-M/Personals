using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Personals.UI.Interop;

public static class ClipboardInterop
{
    public static ValueTask CopyToClipboard(this IJSRuntime jsRuntime, string text)
    {
        return jsRuntime.InvokeVoidAsync("clipboardUtils.copyText", text);  
    }
    
    public static ValueTask CopyToClipboard(this IJSRuntime jsRuntime, ElementReference element)
    {
        return jsRuntime.InvokeVoidAsync("clipboardUtils.copyTextFromElement", element);
    }
}