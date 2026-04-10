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

    private enum APIRequest
    {
        GET,
    }
    
    public DataFunctions(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore, ILogger<DeviceFunctions> logger, ApiTokenService tokenService)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _pendingDeviceStore = pendingDeviceStore;
        _logger = logger;
        _tokenService = tokenService;
    }

    private string FetchUrl(APIRequest req, string param = "")
    {
        switch(req)
        {
            case APIRequest.GET:
                return $"/Data/sensorData/{param}";
        }

        return "";
    }

    public async Task<List<SensorReading>> GetData(DataRetrieval request)
    {
        await _tokenService.AttachToken(_httpClient);

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            _logger.LogError("No device ID specified");
            return new List<SensorReading>();
        }
        
        string encodedDevId = Uri.EscapeDataString(request.DeviceId);
        string start = request.StartDate.ToString("yyyy/MM/dd");
        string end = request.EndDate.ToString("yyyy/MM/dd");

        string url = FetchUrl(APIRequest.GET, $"{encodedDevId}?page={request.Page}&startDate={start}&endDate={end}");
                
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string result = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            _logger.LogError("Request: {request}", JsonConvert.SerializeObject(request));
            return new List<SensorReading>();
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
        await _tokenService.AttachToken(_httpClient);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogError("No device ID specified");
            return new SensorReading();
        }
        
        string encodedDevId = Uri.EscapeDataString(deviceId);
        string url = FetchUrl(APIRequest.GET, $"{encodedDevId}?page=1");
        
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        string result = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"{response.StatusCode.ToString()} - [{url}]");
            _logger.LogError($"Request: {deviceId}");
            return null;
        }
        
        SensorReading reading = JsonConvert.DeserializeObject<SensorReading>(result) ?? new  SensorReading();

        if (reading == null)
        {
            _logger.LogError("No sensor data found");
            return new SensorReading();
        }
        
        return reading;
    }
}