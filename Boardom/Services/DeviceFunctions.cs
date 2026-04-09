using Newtonsoft.Json;
using Boardom.Models;
using System.Text;

namespace Boardom.Services;

public sealed class DeviceFunctions
{
    private readonly HttpClient _httpClient;
    private readonly PendingDeviceStore _pendingDeviceStore;
    private readonly ApiTokenService _tokenService;
    private readonly ILogger<DeviceFunctions> _logger;

    
    private enum APIRequest
    {
        GET,
        GET_ALL,
        ADD,
        EDIT,
        DELETE,
    }

    public DeviceFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore, ILogger<DeviceFunctions> logger, ApiTokenService tokenService)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _pendingDeviceStore = pendingDeviceStore;
        _logger = logger;
        _tokenService = tokenService;
    }

    private string FetchUrl(APIRequest req, string param = "")
    {
        switch (req)
        {
            case APIRequest.GET:    
            case APIRequest.DELETE:
                return $"Device/{param}";
            case APIRequest.GET_ALL:
                return $"Device/getAll";
            case APIRequest.ADD:
                return $"Device/add";
            case APIRequest.EDIT:
                return $"Device/edit";
        }
        return "";
    }
        
    public async Task<List<Device>> GetAllDevices()
    {
        await _tokenService.AttachToken(_httpClient);

        string url = FetchUrl(APIRequest.GET_ALL);

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            return new List<Device>();
        }
        
        string result = await response.Content.ReadAsStringAsync();
        
        List<Device> devices = JsonConvert.DeserializeObject<List<Device>>(result) ?? new List<Device>();
        return devices;
    }

    public async Task<ReturnStatusObject> AddDevice(DeviceAdd request)
    {
        await _tokenService.AttachToken(_httpClient);

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

        string url = FetchUrl(APIRequest.GET, encodedDevId);
        
        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            _pendingDeviceStore.Clear();
            returnStatus.Message = "Device already exists!";
            return returnStatus;
        }

        url = FetchUrl(APIRequest.ADD);
        
        response = await _httpClient.PostAsJsonAsync(url, request);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            _pendingDeviceStore.Clear();
            returnStatus.Success = true;
            returnStatus.Message = result;
            return returnStatus;
        }

        returnStatus.Message = "Failed to add Device";
        _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
        _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
        return returnStatus;
    }

    public async Task<ReturnStatusObject> EditDevice(DeviceEdit request)
    {
        await _tokenService.AttachToken(_httpClient);

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

        string url = FetchUrl(APIRequest.EDIT);
        
        HttpResponseMessage response = await _httpClient.PutAsync(url, content);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
            return returnStatus;
        }
        
        returnStatus.Message = "Failed to edit Device";
        _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
        _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
        return returnStatus;
    }
    
    public async Task<ReturnStatusObject> DeleteDevice(DeviceDelete request)
    {
        await _tokenService.AttachToken(_httpClient);

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
        string url = FetchUrl(APIRequest.DELETE, encodedDeviceId);
        
        HttpResponseMessage response = await _httpClient.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            returnStatus.Message = "Failed to delete Device";
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
            return returnStatus;
        }
        
        returnStatus.Success = true;
        returnStatus.Message = "Device Deleted";
        return returnStatus;
    }
}