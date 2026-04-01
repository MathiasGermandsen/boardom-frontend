using System.Text;
using Newtonsoft.Json;
using Boardom.Models;

namespace Boardom.Services;

public class GroupFunctions
{
    private readonly HttpClient _httpClient;
    private readonly PendingDeviceStore _pendingDeviceStore;
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
    
    public GroupFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore, ILogger<DeviceFunctions> logger)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _pendingDeviceStore = pendingDeviceStore;
        _logger = logger;
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
        HttpResponseMessage response = await _httpClient.GetAsync(FetchUrl(APIRequest.GET_ALL));
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} {response.ReasonPhrase}");
            return null;
        }
        
        string result = await response.Content.ReadAsStringAsync();
        List<Group> groups = JsonConvert.DeserializeObject<List<Group>>(result);
        return groups;
    }

    public async Task<ReturnStatusObject> CreateGroup(GroupCreate request)
    {
        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;
        
        if (string.IsNullOrEmpty(request.Name))
        {
            returnStatus.Message = "Group Name is required";
            return returnStatus;
        }
        
        string json =  JsonConvert.SerializeObject(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = await _httpClient.PostAsync(FetchUrl(APIRequest.CREATE), content);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
        }
        else
        {
            returnStatus.Message = "Failed to create group";
        }
        
        return returnStatus;
    }

    public async Task<ReturnStatusObject> EditGroup(GroupEdit request)
    {
        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.NewName))
        {
            returnStatus.Message = "Group Names are required";
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
        }
        else
        {
            returnStatus.Message = "Failed to edit group";
        }

        return returnStatus;
    }

    public async Task<ReturnStatusObject> DeleteGroup(string name)
    {
        ReturnStatusObject returnStatus = new ReturnStatusObject();
        returnStatus.Success = false;
        
        if (string.IsNullOrWhiteSpace(name))
        {
            returnStatus.Message = "Group Name is required";
            return returnStatus;
        }

        string encodedName = Uri.EscapeDataString(name);

        HttpResponseMessage response = await _httpClient.DeleteAsync(FetchUrl(APIRequest.DELETE, encodedName));
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
        }
        else
        {
            returnStatus.Message = "Failed to delete group";
        }

        return returnStatus;
    }

    public async Task<ReturnStatusObject> AddDeviceToGroup(GroupManageDevice request)
    {
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

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(FetchUrl(APIRequest.ADD_DEVICE), request);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            returnStatus.Success = true;
            returnStatus.Message = result;
        }
        else
        {
            returnStatus.Message = "Failed to add device";
        }

        return returnStatus;
    }

    public async Task<ReturnStatusObject> RemoveDeviceFromGroup(GroupManageDevice request)
    {
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

        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Delete, FetchUrl(APIRequest.DELETE_FROM))
        {
            Content = JsonContent.Create(request)
        };

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
        }

        return returnStatus;
    }
}