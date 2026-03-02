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

    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; set; } = string.Empty;
}