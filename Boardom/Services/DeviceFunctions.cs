using Boardom.Models;

namespace Boardom.Services;

public sealed class DeviceFunctions
{
    private readonly HttpClient _httpClient;
    private readonly PendingDeviceStore _pendingDeviceStore;

    public DeviceFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _pendingDeviceStore = pendingDeviceStore;
    }

    public async Task<(bool Success, string Message)> AddDeviceAsync(DeviceAddRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
            return (false, "Device ID is required");

        if (string.IsNullOrWhiteSpace(request.FriendlyName))
            return (false, "Friendly name is required");

        string encodedDeviceId = Uri.EscapeDataString(request.DeviceId);

        using HttpResponseMessage getResponse = await _httpClient.GetAsync($"Device/{encodedDeviceId}");
        if (getResponse.IsSuccessStatusCode)
        {
            _pendingDeviceStore.Clear();
            return (true, "Device already exists");
        }

        HttpResponseMessage postResponse = await _httpClient.PostAsJsonAsync("Device/add", request);
        string result = await postResponse.Content.ReadAsStringAsync();

        if (postResponse.IsSuccessStatusCode)
        {
            _pendingDeviceStore.Clear();
            return (true, result);
        }

        return (false, "Failed to add device");
    }

    public async Task <List<DeviceInfo>> GetDevicesAsync()
    {
        using HttpResponseMessage response = await _httpClient.GetAsync("Device");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<DeviceInfo>>() ?? new();
    }

    public async Task<(bool success, string Message)> EditDeviceAsync(DeviceEditRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
            return (false, "Device ID is required");

        if (string.IsNullOrWhiteSpace(request.FriendlyName))
            return (false, "Friendly name is required"); 

        using HttpResponseMessage response = await _httpClient.PutAsJsonAsync("Device/edit", request);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return (true, result);

        return (false, "Failed to edit device");
    }

    public async Task<(bool Success, string Message)> SetDeviceActiveAsync(DeviceDeleteRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
            return (false, "Device ID is required");
        
        string encodedDeviceId = Uri.EscapeDataString(request.DeviceId);
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"Device/delete/{encodedDeviceId}", new{IsActive = request.IsActive});
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return (true, result);

        return (false, "failed to update device delete state");
    }
}
