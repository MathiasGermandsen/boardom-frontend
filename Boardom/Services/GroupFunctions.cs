using System.Text;
using Newtonsoft.Json;
using Boardom.Models;

namespace Boardom.Services;

public class GroupFunctions
{
    private readonly HttpClient _httpClient;
    private readonly PendingDeviceStore _pendingDeviceStore;
    private readonly ApiTokenService _tokenService;
    private readonly ILogger<DeviceFunctions> _logger;
    
    private enum APIRequest
    {
        GET_ALL,
        CREATE,
        ADD_DEVICE,
        EDIT,
        DELETE,
        DELETE_FROM
    }
    
    public GroupFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore, ILogger<DeviceFunctions> logger, ApiTokenService tokenService)
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
            case APIRequest.GET_ALL:
                return $"Group/getAll";
            case APIRequest.CREATE:
                return $"Group/create";
            case APIRequest.ADD_DEVICE:
                return $"Group/addDevice";
            case APIRequest.EDIT:
                return $"Group/edit";
            case APIRequest.DELETE:
                return $"Group/{param}";
            case APIRequest.DELETE_FROM:
                return $"Group/deleteFrom";
        }

        return "";
    }

    public async Task<List<Group>> GetAllGroups()
    {
        await _tokenService.AttachToken(_httpClient);

        string url = FetchUrl(APIRequest.GET_ALL);

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            return new List<Group>();
        }
        
        string result = await response.Content.ReadAsStringAsync();
        List<Group> groups = JsonConvert.DeserializeObject<List<Group>>(result);
        return groups;
    }

    public async Task<ReturnStatusObject> CreateGroup(GroupCreate request)
    {
        await _tokenService.AttachToken(_httpClient);

        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;
        
        if (string.IsNullOrEmpty(request.Name))
        {
            returnStatus.Message = "Group Name is required";
            return returnStatus;
        }
        
        string json = JsonConvert.SerializeObject(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        string url = FetchUrl(APIRequest.CREATE);
        
        HttpResponseMessage response = await _httpClient.PostAsync(url, content);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
        }
        else
        {
            returnStatus.Message = "Failed to create group";
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
        }
        
        return returnStatus;
    }

    public async Task<ReturnStatusObject> EditGroup(GroupEdit request)
    {
        await _tokenService.AttachToken(_httpClient);

        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.NewName))
        {
            returnStatus.Message = "Group Names are required";
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
        }
        else
        {
            returnStatus.Message = "Failed to edit group";
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
        }

        return returnStatus;
    }

    public async Task<ReturnStatusObject> DeleteGroup(string name)
    {
        await _tokenService.AttachToken(_httpClient);

        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;
        
        if (string.IsNullOrWhiteSpace(name))
        {
            returnStatus.Message = "Group Name is required";
            return returnStatus;
        }

        string encodedName = Uri.EscapeDataString(name);
        string url = FetchUrl(APIRequest.DELETE, encodedName);

        HttpResponseMessage response = await _httpClient.DeleteAsync(url);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
        }
        else
        {
            returnStatus.Message = "Failed to delete group";
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            _logger.LogError($"Request: {name}");
        }

        return returnStatus;
    }

    public async Task<ReturnStatusObject> AddDeviceToGroup(GroupManageDevice request)
    {
        await _tokenService.AttachToken(_httpClient);

        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            returnStatus.Message = "Group name is required";
            return returnStatus;
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            returnStatus.Message = "Device ID is required";
            return returnStatus;
        }

        string json = JsonConvert.SerializeObject(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        string url = FetchUrl(APIRequest.ADD_DEVICE);

        HttpResponseMessage response = await _httpClient.PostAsync(url, content);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
        }
        else
        {
            if (result.Contains("already"))
            {
                returnStatus.Message = "Device is already in group";
            }
            else
            {
                returnStatus.Message = "Failed to add device";
                _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
                _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
            }
        }

        return returnStatus;
    }

    public async Task<ReturnStatusObject> RemoveDeviceFromGroup(GroupManageDevice request)
    {
        await _tokenService.AttachToken(_httpClient);
        
        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            returnStatus.Message = "Group name is required";
            return returnStatus;
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            returnStatus.Message = "Device ID is required";
            return returnStatus;
        }

        string json = JsonConvert.SerializeObject(request);

        HttpRequestMessage httpRequest = new HttpRequestMessage();

        string url = FetchUrl(APIRequest.DELETE_FROM);

        string constructedUrl = _httpClient.BaseAddress + url;
        
        try
        {
            httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(constructedUrl),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);    
        }
        
        HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
        }
        else
        {
            returnStatus.Message = "Failed to remove device from group";
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
        }

        return returnStatus;
    }
}