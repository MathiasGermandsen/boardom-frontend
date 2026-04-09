using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Boardom.Components.Pages;
using Microsoft.AspNetCore.Mvc;
using Boardom.Models;
using Boardom.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http.HttpResults;

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
public IActionResult Connect([FromBody] DeviceConnect request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.deviceId))
    return BadRequest(new { success = false, message = "Device ID is required"});

    _pendingDeviceStore.SetConnected(request.deviceId);
    return Ok(new { success = true }); // add return jwt too
  }
}