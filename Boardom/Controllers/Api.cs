using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Boardom.Components.Pages;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api")]
[IgnoreAntiforgeryToken]
public class DeviceController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public DeviceController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
    }

[HttpPost("heartbeat")]
  public async Task <IActionResult> Heartbeat([FromBody] DeviceIdBody request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
    {
      return BadRequest(new { success = false, message = "Device ID is required" });
    }
    try
        {

            string encodedDeviceId = Uri.EscapeDataString(request.DeviceId);
            using HttpResponseMessage response = await _httpClient.GetAsync($"Device/{encodedDeviceId}");

            return Ok(new {success = response.IsSuccessStatusCode });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, ex.Message});
        }
}

[HttpPost("addDevice")]
public async Task<IActionResult> AddDevice([FromBody] DeviceIdBody request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
    {
      return BadRequest(new { success = false, message = "Device ID is required"});
    }
    try
    {
      string encodedDeviceId = Uri.EscapeDataString(request.DeviceId);
      using HttpResponseMessage getResponse = await _httpClient.GetAsync($"Device/{encodedDeviceId}");

      if (getResponse.IsSuccessStatusCode)
      {
        return Ok(new { success = true});
      }

      HttpResponseMessage postResponse = await _httpClient.PostAsJsonAsync("Device/addDevice", request);

      if (postResponse.IsSuccessStatusCode)
      {
        string result = await postResponse.Content.ReadAsStringAsync();
        return Ok(new { success = true, data = result });
      }
      else
      {
        return StatusCode((int)postResponse.StatusCode, new { success = false, message = "Failed to add device" });
      }
    }
    catch (Exception ex)
    {
      return StatusCode(500, new { success = false, ex.Message});
    }
  }


public class DeviceIdBody
{
  public string DeviceId { get; set; } = string.Empty; 
}

}