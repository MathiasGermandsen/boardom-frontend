using Newtonsoft.Json;
using Boardom.Models;
using System.Text;

namespace Boardom.Services;

public sealed class DeviceFunctions
{
    private readonly HttpClient _httpClient;
    private readonly PendingDeviceStore _pendingDeviceStore;
    private readonly ILogger<DeviceFunctions> _logger;
    
    private enum RequestType
    {
        GET,
        POST,
        PUT,
        DELETE,
    }

    public DeviceFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore, ILogger<DeviceFunctions> logger)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _pendingDeviceStore = pendingDeviceStore;
        _logger = logger;
    }

    private string FetchUrl(RequestType req, string param = "")
    {
        switch (req)
        {
            case RequestType.GET:
            case RequestType.DELETE:    
                return $"Device/{param}";
            case RequestType.POST:
                return $"Device/add";
            case RequestType.PUT:
                return $"Device/edit";
        }
        return "";
    }
        
    public async Task<List<Device>> GetAllDevices()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(FetchUrl(RequestType.GET, "getAll"));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} {response.ReasonPhrase}");
            return null;
        }
        
        string result = await response.Content.ReadAsStringAsync();
        
        List<Device> devices = JsonConvert.DeserializeObject<List<Device>>(result) ?? new List<Device>();
        return devices;
    }

    public async Task<ReturnStatusObject> AddDevice(DeviceAdd request)
    {
        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;
        
        if (request == null)
        {
            returnStatus.Message = "Request is null";
            return returnStatus;
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            returnStatus.Message = "Device ID is empty";
            return returnStatus;
        }

        if (string.IsNullOrWhiteSpace(request.FriendlyName))
        {
            returnStatus.Message = "Friendly name is empty";
            return returnStatus;
        }
        
        string encodedDevId = Uri.EscapeDataString(request.DeviceId);
        
        HttpResponseMessage response = await _httpClient.GetAsync(FetchUrl(RequestType.GET, encodedDevId));

        if (response.IsSuccessStatusCode)
        {
            _pendingDeviceStore.Clear();
            returnStatus.Message = "Device already exists!";
            return returnStatus;
        }
        
        response = await _httpClient.PostAsJsonAsync(FetchUrl(RequestType.POST), request);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _pendingDeviceStore.Clear();
            returnStatus.Success = true;
            returnStatus.Message = result;
            return returnStatus;
        }

        returnStatus.Message = "Failed to add Device";
        return returnStatus;
    }

    public async Task<ReturnStatusObject> EditDevice(DeviceEdit request)
    {
        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;

        if (request == null)
        {
            returnStatus.Message = "Request is null";
            return returnStatus;
        }
        
        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            returnStatus.Message = "Device ID is empty";
            return returnStatus;
        }

        if (string.IsNullOrWhiteSpace(request.NewFriendlyName))
        {
            returnStatus.Message = "Friendly name is empty";
            return returnStatus;
        }
        
        string json = JsonConvert.SerializeObject(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = await _httpClient.PutAsync(FetchUrl(RequestType.PUT), content);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
            return returnStatus;
        }
        
        returnStatus.Message = "Failed to edit Device";
        return returnStatus;
    }
    
    public async Task<ReturnStatusObject> DeleteDevice(DeviceDelete request)
    {
        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;
        
        if (request == null)
        {
            returnStatus.Message = "Request is null";
            return returnStatus;
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            returnStatus.Message = "Device ID is empty";
            return returnStatus;
        }
        
        string encodedDeviceId = Uri.EscapeDataString(request.DeviceId);
        
        HttpResponseMessage response = await _httpClient.DeleteAsync(FetchUrl(RequestType.DELETE, encodedDeviceId));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} {response.ReasonPhrase}");
            returnStatus.Message = "Failed to delete Device";
            return returnStatus;
        }
        
        returnStatus.Success = true;
        returnStatus.Message = "Device deleted";
        return returnStatus;
    }
}