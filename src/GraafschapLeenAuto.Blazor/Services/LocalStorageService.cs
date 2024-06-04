using Microsoft.JSInterop;
using System.Text.Json;

namespace GraafschapLeenAuto.Blazor.Services;

public class LocalStorageService(IJSRuntime jsRuntime)
{
    public async Task SetItemAsync(string key, string value)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    //public async Task SetItemAsync<T>(string key, T value)
    //{
    //    var serializedValue = JsonSerializer.Serialize(value);
    //    await SetItemAsync(key, serializedValue);
    //}

    public async Task<string> GetItemAsync(string key)
    {
        return await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    }

    //public async Task<T?> GetItemAsync<T>(string key)
    //{
    //    var item = await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

    //    if (item == null)
    //    {
    //        return default;
    //    }

    //    return JsonSerializer.Deserialize<T>(item);
    //}

    public async Task RemoveItemAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
