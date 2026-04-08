using Newtonsoft.Json;
using Boardom.Models;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace Boardom.Services;

public class DataFunctions
{
    private readonly HttpClient _httpClient;
    private readonly PendingDeviceStore _pendingDeviceStore;
    private readonly ApiTokenService _tokenService;
    private readonly ILogger<DeviceFunctions> _logger;
    
    public DataFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore, ILogger<DeviceFunctions> logger, ApiTokenService tokenService)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _pendingDeviceStore = pendingDeviceStore;
        _logger = logger;
        _tokenService = tokenService;
    }

    public async Task<List<SensorReading>> GetData(DataRetrieval request)
    {
        await AttachTokenAsync();

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            _logger.LogError("No device ID specified");
            return null;
        }
        
        string encodedDevId = Uri.EscapeDataString(request.DeviceId);
        string start = request.StartDate.ToString("yyyy/MM/dd");
        string end = request.EndDate.ToString("yyyy/MM/dd");
        
        string url = $"Data/sensorData/{encodedDevId}?page={request.Page}&startDate={start}&endDate={end}";
        _logger.LogInformation($"Fetching sensor data from: {url}");
        
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string result = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Error fetching sensor data: {result}");
            return null;
        }

        List<SensorReading> readings = new List<SensorReading>();

        try
        {
            readings = JsonConvert.DeserializeObject<List<SensorReading>>(result) ?? new List<SensorReading>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: No Data {ex.Message}");
        }
            
        if (readings?.Count > 0)
        {
            return readings!;
        }
        else
        {
            _logger.LogError("No sensor data found");
        }

        return readings;
    }

    public async Task<SensorReading?> GetLatestSensorDataAsync(string deviceId)
    {
        await AttachTokenAsync();

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogWarning("[DEBUG] GetLatestSensorDataAsync called with empty deviceId");
            return null;
        }
        
        string encodedDevId = Uri.EscapeDataString(deviceId);
        string url = $"Data/sensorData/{encodedDevId}?page=1";
        
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string result = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Error fetching sensor data: {result}");
            return null;
        }
        
        SensorReading reading = JsonConvert.DeserializeObject<SensorReading>(result) ?? new  SensorReading();

        if (reading == null)
        {
            _logger.LogError("No sensor data found");
            return null;
        }
        
        return reading;
    }

    //JWT token method
    private async Task AttachTokenAsync()
    {
        string? token = await _tokenService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}