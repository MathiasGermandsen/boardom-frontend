using Newtonsoft.Json;

namespace Boardom.Models;


// ================= API Request Models =================

public sealed class DeviceConnect
{
    [JsonProperty("deviceId")] 
    public string Id { get; set; } = string.Empty;
}

public sealed class DeviceAdd
{
    [JsonProperty("deviceId")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("friendlyName")]
    public string FriendlyName { get; set; } = string.Empty;
}

public sealed class DeviceDelete
{
    [JsonProperty("deviceId")]
    public string Id { get; set; } = string.Empty;
}

public sealed class DeviceEdit
{
    [JsonProperty("deviceId")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("newFriendlyName")]
    public string NewFriendlyName { get; set; } = string.Empty;
}

// ================= Device Models =================

public sealed class Device
{
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; } = string.Empty;
    
    [JsonProperty("friendlyName")]
    public string FriendlyName { get; set; } = string.Empty;
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty("lastHeartbeat")]
    public DateTime LastHeartbeat { get; set; }
    
    [JsonProperty("latestSensorReading")]
    public SensorReading LatestReading { get; set; } = new SensorReading();

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
}