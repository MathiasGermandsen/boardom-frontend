using System.Text.Json;
using Boardom.Models;

namespace Boardom.Services;

public sealed class DeviceFunctions
{
  private readonly HttpClient _httpClient;
  private readonly PendingDeviceStore _pendingDeviceStore;
  private readonly ILogger<DeviceFunctions> _logger;

  public DeviceFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore, ILogger<DeviceFunctions> logger)
  {
    _httpClient = httpClientFactory.CreateClient("DatabaseApi");
    _pendingDeviceStore = pendingDeviceStore;
    _logger = logger;
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

  public async Task<List<DeviceGroup>> GetDeviceGroupsAsync()
  {
    try
    {
      var raw = await _httpClient.GetStringAsync("Group/getAll");
      _logger.LogInformation("[DEBUG] Group/getAll raw JSON: {Json}", raw);
      return JsonSerializer.Deserialize<List<DeviceGroup>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
             ?? new List<DeviceGroup>();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[DEBUG] Group/getAll failed");
      return new List<DeviceGroup>();
    }
  }

  public async Task<List<Device>> GetAllDevicesAsync()
  {
    try
    {
      var raw = await _httpClient.GetStringAsync("Device/getAll");
      _logger.LogInformation("[DEBUG] Device/getAll raw JSON: {Json}", raw);
      return JsonSerializer.Deserialize<List<Device>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
             ?? new List<Device>();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[DEBUG] Device/getAll failed");
      return new List<Device>();
    }
  }

  public async Task<SensorReading?> GetLatestSensorDataAsync(string deviceId)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
    {
      _logger.LogWarning("[DEBUG] GetLatestSensorDataAsync called with empty deviceId");
      return null;
    }

    try
    {
      string encoded = Uri.EscapeDataString(deviceId);
      string url = $"Data/sensorData/{encoded}?page=1";
      _logger.LogInformation("[DEBUG] Fetching sensor data from: {Url}", url);

      var response = await _httpClient.GetAsync(url);
      var raw = await response.Content.ReadAsStringAsync();
      _logger.LogInformation("[DEBUG] Data/sensorData/{DeviceId} status={Status}, body={Json}",
        deviceId, response.StatusCode, raw.Length > 500 ? raw[..500] : raw);

      if (!response.IsSuccessStatusCode) return null;

      var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

      try
      {
        var paginated = JsonSerializer.Deserialize<PaginatedSensorResponse>(raw, options);
        if (paginated?.Data?.Count > 0)
        {
          // Skip entries where all sensor values are zero (heartbeat pings)
          var real = paginated.Data.FirstOrDefault(r =>
            r.Temperature != 0 || r.Humidity != 0 || r.Light != 0 || r.Pressure != 0);
          return real ?? paginated.Data.First();
        }
      }
      catch { }

      try
      {
        var list = JsonSerializer.Deserialize<List<SensorReading>>(raw, options);
        if (list?.Count > 0) return list.First();
      }
      catch { }

      return JsonSerializer.Deserialize<SensorReading>(raw, options);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[DEBUG] Data/sensorData/{DeviceId} failed", deviceId);
      return null;
    }
    
  }

    public async Task <List<DeviceInfo>> GetDevicesAsync()
    {
        using HttpResponseMessage response = await _httpClient.GetAsync("Device/getAll");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<DeviceInfo>>() ?? new();
    }

    public async Task<(bool success, string Message)> EditDeviceAsync(DeviceEditRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
            return (false, "Device ID is required");

        if (string.IsNullOrWhiteSpace(request.FriendlyName))
            return (false, "Friendly name is required"); 

        _logger.LogInformation("[DEBUG] EditDevice request: DeviceId={DeviceId}, FriendlyName={friendlyName}",
        request.DeviceId, request.FriendlyName);

        using HttpResponseMessage response = await _httpClient.PutAsJsonAsync("Device/edit", request);
        string result = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("[DEBUG] EditDevice response: Status={Status}, Body={Body}", 
            response.StatusCode, result);

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

