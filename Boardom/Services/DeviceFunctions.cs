using Newtonsoft.Json;
using Boardom.Models;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

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
    if (request == null)
    {
      return (false, "Request is null");
    }

    if (string.IsNullOrWhiteSpace(request.DeviceId))
    {
      return (false, "Device ID is required");
    }

    if (string.IsNullOrWhiteSpace(request.FriendlyName))
    {
      return (false, "Friendly Name is required");
    }  

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
      string raw = await _httpClient.GetStringAsync("Group/getAll");

      List<DeviceGroup> groupList = JsonConvert.DeserializeObject<List<DeviceGroup>>(raw) ?? new List<DeviceGroup>();

      return groupList;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Group/getAll failed");
      return new List<DeviceGroup>();
    }
  }

  public async Task<List<Device>> GetAllDevicesAsync()
  {
    try
    {
      string raw = await _httpClient.GetStringAsync("Device/getAll");

      List<Device> devList = JsonConvert.DeserializeObject<List<Device>>(raw) ?? new List<Device>();

      return devList!;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Device/getAll failed");
      return new List<Device>();
    }
  }


  public async Task<List<SensorReading>> GetFilteredSensorDataAsync(string deviceId, string dataType, DateTime startDate, DateTime endDate, int page)
  {
    if (string.IsNullOrWhiteSpace(deviceId))
    {
      _logger.LogWarning("[DEBUG] GetFilteredSensorDataAsync called with empty deviceId");
      return new List<SensorReading>();
    }

    try
    {
      string encodedId = Uri.EscapeDataString(deviceId);
      string start = startDate.Date.ToString("yyyy/MM/dd");
      string end = endDate.Date.ToString("yyyy/MM/dd");

      string url = $"Data/sensorData/{encodedId}?page={page}&startDate={start}&endDate={end}";
      _logger.LogInformation("[DEBUG] Fetching sensor data from: {Url}", url);

      HttpResponseMessage response = await _httpClient.GetAsync(url);
      string raw = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode) return new List<SensorReading>();

      SensorDataResponse res = JsonConvert.DeserializeObject<SensorDataResponse>(raw) ?? new SensorDataResponse();

      if (res.Data?.Count > 0)
      {
        return res.Data!;
      }
      else
      {
        _logger.LogError("No data");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to read sensordata");
    }

    return new List<SensorReading>();
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

      HttpResponseMessage response = await _httpClient.GetAsync(url);
      string raw = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode) return null;

      SensorDataResponse res = JsonConvert.DeserializeObject<SensorDataResponse>(raw) ?? new SensorDataResponse();

      if (res.Data?.Count > 0)
      {
        return res.Data![0];
      }
      else
      {
        _logger.LogError("No Data");
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to read sensordata");
    }    

    return new SensorReading();
  }

  public async Task<List<DeviceInfo>> GetDevicesAsync()
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

      _logger.LogInformation("[DEBUG] EditDevice request: DeviceId: {DeviceId}, FriendlyName: {friendlyName}",
      request.DeviceId, request.FriendlyName);

      string json = JsonConvert.SerializeObject(request);
      StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

      HttpResponseMessage response = await _httpClient.PutAsync("Device/edit", content);
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
      using HttpResponseMessage response = await _httpClient.DeleteAsync($"Device/{encodedDeviceId}");
      string result = await response.Content.ReadAsStringAsync();

      if (response.IsSuccessStatusCode)
          return (true, result);

      return (false, "failed to update device delete state");
  }

  public async Task<(bool Success, string Message)> CreateGroupAsync(GroupCreateRequest request)    
  {
      if (string.IsNullOrWhiteSpace(request.GroupName))
        return (false, "Group name is required ");

      string json = JsonConvert.SerializeObject(request);
      StringContent sContent = new StringContent(json, Encoding.UTF8, "application/json");

      using HttpResponseMessage response = await _httpClient.PostAsync("Group/create", sContent);
      string result = await response.Content.ReadAsStringAsync();

      return response.IsSuccessStatusCode
        ? (true, result)
        : (false, "Failed to edit group");
    }
    
    public async Task<(bool Success, string Message)> EditGroupAsync(GroupEditRequest request)
    {
      if (string.IsNullOrWhiteSpace(request.GroupName) || string.IsNullOrWhiteSpace(request.NewGroupName))
        return (false, "Group names are required");

        string json = JsonConvert.SerializeObject(request);
        StringContent sContent = new StringContent(json, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _httpClient.PutAsync("Group/edit", sContent);
        string result = await response.Content.ReadAsStringAsync();

        return response.IsSuccessStatusCode
        ? (true, result)
        : (false, "Failed to edit group");
    }

  public async Task<(bool Success, string Message)> DeleteGroupAsync (string groupName)
  {
    if (string.IsNullOrWhiteSpace(groupName))
      return (false, "Groupd name is required");

    string encoded = Uri.EscapeDataString(groupName);
    using HttpResponseMessage response = await _httpClient.DeleteAsync($"Group/{encoded}");
    string result = await response.Content.ReadAsStringAsync();

    return response.IsSuccessStatusCode
      ? (true, result)
      : (false, "Failed to delete group");
  }

  public async Task<(bool Success, string Message)> AddDeviceToGroupAsync(GroupAddDeviceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.GroupName) || string.IsNullOrWhiteSpace(request.DeviceId))
      return (false, "Group name and device id are required");
    
    using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("Group/addDevice", request);
    string result = await response.Content.ReadAsStringAsync();

    return response.IsSuccessStatusCode
      ? (true, result)
      : (false, "Failed to add device to group");
  }

  public async Task<(bool Success, string Message)> RemoveDeviceFromGroupAsync(GroupAddDeviceRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.GroupName) || string.IsNullOrWhiteSpace(request.DeviceId))
      return (false, "Group namde and device id are required");

    var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "Group/deleteFrom")
    {
      Content = JsonContent.Create(request)
    };
    using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);
    string result = await response.Content.ReadAsStringAsync();

    return response.IsSuccessStatusCode
      ? (true, result)
      : (false, "Failed to remove device from group");
  }


}


  

