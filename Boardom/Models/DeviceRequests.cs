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

    [JsonPropertyName("friendlyName")]
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