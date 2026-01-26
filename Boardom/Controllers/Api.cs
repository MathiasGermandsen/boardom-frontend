using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Boardom.Components.Pages;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api")]
public class HeartbeatController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public HeartbeatController(IHttpClientFactory httpClientFactory)
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
            using var response = await _httpClient.GetAsync($"Device/{encodedDeviceId}");

            return Ok(new {success = response.IsSuccessStatusCode });
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