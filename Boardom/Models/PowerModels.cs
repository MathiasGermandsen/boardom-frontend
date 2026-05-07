using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Boardom.Models;

public sealed class PowerObject
{
    [JsonProperty("selectionMode")]
    public string Selection { get; set; } = string.Empty;

    [JsonProperty("company")]
    public string Company { get; set; } = string.Empty;

    [JsonProperty("numberOfHours")]
    public int Hours { get; set; }
    
    [JsonProperty("price")]
    public double MaxPrice { get; set; }

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
}

public class Company
{
  [JsonProperty("name")]
  public string Name { get; set; }
}