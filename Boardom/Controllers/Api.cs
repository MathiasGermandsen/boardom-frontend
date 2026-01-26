using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
            var response = await _httpClient.GetAsync($"Device/{request.DeviceId}");

            if (response.IsSuccessStatusCode)
            {
                return Ok(new {success = true});
            }
            else { return Ok(new {success = false}); 
            }
        }
        catch (Exception ex)
        {
            return Ok(new {success = false });
        }
}

public class DeviceIdBody
{
  public string DeviceId { get; set; } = string.Empty; 
}

}