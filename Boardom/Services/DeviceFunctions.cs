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
        await AttachTokenAsync();
        HttpResponseMessage response = await _httpClient.GetAsync(FetchUrl(APIRequest.GET_ALL));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} - [{FetchUrl(APIRequest.GET_ALL)}]");
            return new List<Device>();
        }
        
        string result = await response.Content.ReadAsStringAsync();
        
        List<Device> devices = JsonConvert.DeserializeObject<List<Device>>(result) ?? new List<Device>();
        return devices;
    }

    public async Task<ReturnStatusObject> AddDevice(DeviceAdd request)
    {
        await AttachTokenAsync();

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
        
        HttpResponseMessage response = await _httpClient.GetAsync(FetchUrl(APIRequest.GET, encodedDevId));

        if (response.IsSuccessStatusCode)
        {
            _pendingDeviceStore.Clear();
            returnStatus.Message = "Device already exists!";
            return returnStatus;
        }
        
        response = await _httpClient.PostAsJsonAsync(FetchUrl(APIRequest.ADD), request);
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
        await AttachTokenAsync();

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
        
        HttpResponseMessage response = await _httpClient.PutAsync(FetchUrl(APIRequest.EDIT), content);
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
        await AttachTokenAsync();

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
        
        HttpResponseMessage response = await _httpClient.DeleteAsync(FetchUrl(APIRequest.DELETE, encodedDeviceId));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} - [{FetchUrl(APIRequest.DELETE)}]");
            returnStatus.Message = "Failed to delete Device";
            return returnStatus;
        }
        
        returnStatus.Success = true;
        returnStatus.Message = "Device deleted";
        return returnStatus;
    }

    //JWT Token method
    private async Task AttachTokenAsync()
    {
        string? token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}