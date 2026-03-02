using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Boardom.Components.Pages;
using Microsoft.AspNetCore.Mvc;
using Boardom.Models;
using Boardom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;

[ApiController]
[Route("api")]
[IgnoreAntiforgeryToken]
public class DeviceController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly PendingDeviceStore _pendingDeviceStore;

    public DeviceController(IHttpClientFactory httpClientFactory, PendingDeviceStore pendingDeviceStore)
    {
        _httpClient = httpClientFactory.CreateClient("DatabaseApi");
        _pendingDeviceStore = pendingDeviceStore;
    }

[AllowAnonymous]
[HttpPost("connect")]
public IActionResult Connect([FromBody] DeviceConnectRequest request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
    return BadRequest(new { success = false, message = "Device ID is required"});

    _pendingDeviceStore.SetConnected(request.DeviceId);
    return Ok(new { success = true });
  }

[HttpPost("heartbeat")]
  public async Task <IActionResult> Heartbeat([FromBody] DeviceConnectRequest request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
      return BadRequest(new { success = false, message = "Device ID is required" });

    try
        {
            string encodedDeviceId = Uri.EscapeDataString(request.DeviceId);
            using HttpResponseMessage response = await _httpClient.GetAsync($"Device/{encodedDeviceId}");

            if (response.IsSuccessStatusCode)
              return Ok(new {success = true });            

              return StatusCode((int)response.StatusCode, new {success = false, message = "Device not found"});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {success = false, ex.Message});
        }
}

[HttpPost("addDevice")]
public async Task<IActionResult> AddDevice([FromBody] DeviceAddRequest request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
    {
      return BadRequest(new { success = false, message = "Device ID is required"});
    }

    if (string.IsNullOrWhiteSpace(request.FriendlyName))
    {
      return BadRequest(new { success = false, message = "Friendly name is required "});
    }

    try
    {
      string encodedDeviceId = Uri.EscapeDataString(request.DeviceId);

      using HttpResponseMessage getResponse = await _httpClient.GetAsync($"Device/{encodedDeviceId}");
      if (getResponse.IsSuccessStatusCode)
      {
        _pendingDeviceStore.Clear();
        return Ok(new { success = true});
      }

      HttpResponseMessage postResponse = await _httpClient.PostAsJsonAsync("Device/addDevice", request);

      string result = await postResponse.Content.ReadAsStringAsync();

      if (postResponse.IsSuccessStatusCode)
      {
        _pendingDeviceStore.Clear();
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
}