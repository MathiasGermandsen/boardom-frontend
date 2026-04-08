using Microsoft.Extensions.Hosting;
using Boardom.Models;
using Newtonsoft.Json;
using Boardom.Services;

namespace Boardom.Services;

public class DeviceStatus
{
    private readonly ILogger<DeviceStatus> _logger;
    private readonly HttpClient _httpClient;
    private readonly DeviceFunctions _deviceFunctions;
    private Dictionary<string, bool> _statusList = new Dictionary<string, bool>();

    private Timer? _timer;


    public DeviceStatus(IHttpClientFactory httpClientFactory, ILogger<DeviceStatus> logger, DeviceFunctions deviceFunctions)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _logger = logger;
        _deviceFunctions = deviceFunctions;
    }


    public async Task DeviceCheckAsync()
    {
        _statusList.Clear();

        _logger.LogInformation("Running Device Check");

        List<Device> allDevices = await _deviceFunctions.GetAllDevices();
        
        foreach (Device dev in allDevices)
        {
            bool online = DateTime.UtcNow - dev.LastHeartbeat <= TimeSpan.FromMinutes(4);

            _statusList.Add(dev.DeviceId, online);    
        }
    }

    public bool IsOnline(string deviceId)
    {
        if (!_statusList.Any())
        {
            return false;
        }

        if (!_statusList.Keys.Contains(deviceId))
        {
            return false;
        }

        return _statusList[deviceId];
    }

    public int CountStatus(bool boolValue)
    {
        int count = _statusList.Count(x => x.Value == boolValue);
        return count;
    }
}

