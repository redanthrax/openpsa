using Microsoft.JSInterop;

namespace OpenPsa.Web.Features.Authentication.Services;

public interface ITokenStore {
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task ClearAsync();
}

public class LocalStorageTokenStore : ITokenStore {
    private const string Key = "openpsa_token";
    private readonly IJSRuntime _js;

    public LocalStorageTokenStore(IJSRuntime js) => _js = js;

    public ValueTask<string?> GetTokenAsync() =>
        _js.InvokeAsync<string?>("localStorage.getItem", Key);

    public ValueTask SetTokenAsync(string token) =>
        _js.InvokeVoidAsync("localStorage.setItem", Key, token);

    public ValueTask ClearAsync() =>
        _js.InvokeVoidAsync("localStorage.removeItem", Key);

    Task<string?> ITokenStore.GetTokenAsync() => GetTokenAsync().AsTask();
    Task ITokenStore.SetTokenAsync(string token) => SetTokenAsync(token).AsTask();
    Task ITokenStore.ClearAsync() => ClearAsync().AsTask();
}
