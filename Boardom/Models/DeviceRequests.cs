using System.Text.Json.Serialization;

namespace Boardom.Models;

public sealed class DeviceConnectRequest
{
  [JsonPropertyName("deviceId")]
  public string DeviceId { get; set; } = string.Empty;
}

public sealed class DeviceAddRequest
{
  [JsonPropertyName("deviceId")]
  public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("friendlyName")]
    public string FriendlyName { get; set; } = string.Empty;
}

public sealed class DeviceDeleteRequest
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName ("IsActive")]
    public bool IsActive { get; set;}
}

public sealed class DeviceEditRequest
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("newFriendlyName")]
    public string FriendlyName { get; set; } = string.Empty;
}

public sealed class DeviceInfo
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("friendlyName")]
    public string FriendlyName { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

// ── Dashboard models ──

public class DeviceGroup
{
  [JsonPropertyName("groupName")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("devices")]
  public List<Device> Devices { get; set; } = new();
}

public class Device
{
  [JsonPropertyName("deviceId")]
  public string DeviceId { get; set; } = string.Empty;

  [JsonPropertyName("friendlyName")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("isOnline")]
  public bool IsOnline { get; set; } = true;

  /// <summary>Populated client-side after fetching sensor data.</summary>
  [JsonIgnore]
  public SensorReading? LatestReading { get; set; }
}

public class SensorReading
{
  [JsonPropertyName("light")]
  public double? Light { get; set; }

  [JsonPropertyName("temperature")]
  public double? Temperature { get; set; }

  [JsonPropertyName("humidity")]
  public double? Humidity { get; set; }

  [JsonPropertyName("pressure")]
  public double? Pressure { get; set; }

  [JsonPropertyName("moisture")]
  public double? Moisture { get; set; }
}

/// <summary>Wrapper returned by the paginated sensor-data endpoint.</summary>
public class PaginatedSensorResponse
{
  [JsonPropertyName("items")]
  public List<SensorReading> Data { get; set; } = new();
}
