using Microsoft.Extensions.Hosting;
using Boardom.Models;
using Newtonsoft.Json;
using Boardom.Services;

namespace Boardom.Services;

public class DeviceStatus : BackgroundService
{
    private readonly ILogger<DeviceStatus> _logger;
    private Dictionary<string, bool> _statusList = new Dictionary<string, bool>();

    private DeviceFunctions _deviceFunctions;


    public DeviceStatus(ILogger<DeviceStatus> logger, DeviceFunctions deviceFunctions)
    {
        _logger = logger;
        _deviceFunctions = deviceFunctions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeviceCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background check");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task DeviceCheckAsync(CancellationToken cancellationToken)
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

    public async Task<int> CountStatus(bool boolValue)
    {
        return _statusList.Count(x => x.Value == boolValue);
    }
}

