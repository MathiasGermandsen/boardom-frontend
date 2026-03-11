using Newtonsoft.Json;

namespace Boardom.Models;

public sealed class DeviceConnectRequest
{
  [JsonProperty("deviceId")]
  public string DeviceId { get; set; } = string.Empty;
}

public sealed class DeviceAddRequest
{
  [JsonProperty("deviceId")]
  public string DeviceId { get; set; } = string.Empty;

    [JsonProperty("friendlyName")]
    public string FriendlyName { get; set; } = string.Empty;
}

public sealed class DeviceDeleteRequest
{
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonProperty ("IsActive")]
    public bool IsActive { get; set;}
}

public sealed class DeviceEditRequest
{
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonProperty("newFriendlyName")]
    public string FriendlyName { get; set; } = string.Empty;
}

public sealed class DeviceInfo
{
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonProperty("friendlyName")]
    public string FriendlyName { get; set; } = string.Empty;

    [JsonProperty("isActive")]
    public bool IsActive { get; set; }
    
    [JsonIgnore]
    public SensorReading? LatestReading { get; set; }
}

// ── Dashboard models ──

public class DeviceGroup
{
  [JsonProperty("groupName")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("devices")]
  public List<Device> Devices { get; set; } = new();
}

public class Device
{
  [JsonProperty("deviceId")]
  public string DeviceId { get; set; } = string.Empty;

  [JsonProperty("friendlyName")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("lastHeartbeat")]
  public DateTime LastHeartbeat {get; set;}

  /// <summary>Populated client-side after fetching sensor data.</summary>
  [JsonIgnore]
  public SensorReading? LatestReading { get; set; }
}

public class SensorReading
{
  [JsonProperty("light")]
  public double? Light { get; set; }

  [JsonProperty("temperature")]
  public double? Temperature { get; set; }

  [JsonProperty("humidity")]
  public double? Humidity { get; set; }

  [JsonProperty("pressure")]
  public double? Pressure { get; set; }

  [JsonProperty("moisture")]
  public double? Moisture { get; set; }
  
  [JsonProperty("dateAdded")]
  public DateTime DateAdded { get; set; }
}

/// <summary>Wrapper returned by the paginated sensor-data endpoint.</summary>
public class SensorDataResponse
{
  [JsonProperty("items")]
  public List<SensorReading> Data { get; set; } = new();
}


//Group add/edit/delete and such...
public sealed class GroupCreateRequest
{
  [JsonProperty("groupName")]
  public string GroupName { get; set; } = string.Empty;
}

public sealed class GroupEditRequest
{
  [JsonProperty("groupName")]
  public string GroupName { get; set; } = string.Empty;

  [JsonProperty("newName")]
  public string NewGroupName { get; set;} = string.Empty;
}

public sealed class GroupAddDeviceRequest
{
  [JsonProperty("groupName")]
  public string GroupName { get; set; } = string.Empty;

  [JsonProperty("deviceId")]
  public string DeviceId { get; set; } = string.Empty;
}