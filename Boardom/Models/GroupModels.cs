using Newtonsoft.Json;

namespace Boardom.Models;

// ================= API Request Models =================

public sealed class GroupCreate
{
    [JsonProperty("groupName")]
    public string Name { get; set; } = string.Empty;
}

public sealed class GroupEdit
{
    [JsonProperty("groupName")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("newName")]
    public string NewName { get; set; } = string.Empty;
}

public sealed class GroupManageDevice
{
    [JsonProperty("groupName")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("deviceId")]
    public string DeviceId { get; set; } = string.Empty;
}

// ================= Group Models =================

public sealed class Group
{
    [JsonProperty("groupName")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("devices")]
    public List<Device> Devices { get; set; } = new List<Device>();
}