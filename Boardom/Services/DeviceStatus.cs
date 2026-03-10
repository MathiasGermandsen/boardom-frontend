using Microsoft.Extensions.Hosting;
using Boardom.Models;
using Newtonsoft.Json;

namespace Boardom.Services;

public class DeviceStatus : BackgroundService
{
    private readonly ILogger<DeviceStatus> _logger;
    private readonly HttpClient _httpClient;
    public Dictionary<string, bool> StatusList = new Dictionary<string, bool>();


    public DeviceStatus(IHttpClientFactory httpClientFactory, ILogger<DeviceStatus> logger)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _logger = logger;
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
        StatusList.Clear();

        _logger.LogInformation("Running Device Check");

        string raw = await _httpClient.GetStringAsync("Device/getAll");

        List<Device> allDevices = JsonConvert.DeserializeObject<List<Device>>(raw);

        foreach (Device dev in allDevices)
        {
            bool online = DateTime.UtcNow - dev.LastHeartbeat <= TimeSpan.FromMinutes(4);

            StatusList.Add(dev.DeviceId, online);    
        }
    }
}

