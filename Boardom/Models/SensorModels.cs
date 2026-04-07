using Newtonsoft.Json;

namespace Boardom.Models;

// ================= Sensor Models =================

public sealed class DataRetrieval
{
    public string DeviceId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Page { get; set; }
}

public sealed class SensorReading
{
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; }
    
    [JsonProperty("dateAdded")]
    public DateTime DateAdded { get; set; }
    
    [JsonProperty("temperature")]
    public double Temperature { get; set; }
    
    [JsonProperty("humidity")]
    public double Humidity { get; set; }
    
    [JsonProperty("pressure")]
    public double Pressure { get; set; }
    
    [JsonProperty("light")]
    public double Light { get; set; }
    
    [JsonProperty("moisture")]
    public double Moisture { get; set; }
}
