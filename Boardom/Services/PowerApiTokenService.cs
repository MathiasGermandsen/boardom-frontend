using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Boardom.Services;

public class PowerApiTokenService : IHostedService, IDisposable
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly IConfiguration _configuration;
  private readonly ILogger<PowerApiTokenService> _logger;

  private string? _token;
  private string? _jti;
  private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
  private readonly SemaphoreSlim _lock = new(1, 1);
  private Timer? _rotationTimer;

  // Rotate 1 hour before expiry
  private static readonly TimeSpan RotationLeadTime = TimeSpan.FromHours(1);

  public PowerApiTokenService(
      IHttpClientFactory httpClientFactory,
      IConfiguration configuration,
      ILogger<PowerApiTokenService> logger)
  {
    _httpClientFactory = httpClientFactory;
    _configuration = configuration;
    _logger = logger;
  }

  // ── IHostedService ────────────────────────────────────────────────────────

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    await ObtainTokenAsync(cancellationToken);
    ScheduleRotation();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _rotationTimer?.Change(Timeout.Infinite, 0);
    return Task.CompletedTask;
  }

  public void Dispose()
  {
    _rotationTimer?.Dispose();
    _lock.Dispose();
  }

  // ── Public API ────────────────────────────────────────────────────────────

  /// <summary>Returns a valid bearer token, obtaining one if needed.</summary>
  public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
  {
    if (!string.IsNullOrEmpty(_token) && DateTimeOffset.UtcNow < _expiresAt - RotationLeadTime)
      return _token;

    await _lock.WaitAsync(cancellationToken);
    try
    {
      // Re-check inside the lock
      if (!string.IsNullOrEmpty(_token) && DateTimeOffset.UtcNow < _expiresAt - RotationLeadTime)
        return _token;

      if (!string.IsNullOrEmpty(_jti))
        await RotateTokenAsync(cancellationToken);
      else
        await ObtainTokenAsync(cancellationToken);
    }
    finally
    {
      _lock.Release();
    }

    return _token;
  }

  /// <summary>Forces token rotation (called by handler on 401 response).</summary>
  public async Task ForceRotateAsync(CancellationToken cancellationToken = default)
  {
    await _lock.WaitAsync(cancellationToken);
    try
    {
      if (!string.IsNullOrEmpty(_jti))
        await RotateTokenAsync(cancellationToken);
      else
        await ObtainTokenAsync(cancellationToken);
    }
    finally
    {
      _lock.Release();
    }

    ScheduleRotation();
  }

  // ── Private helpers ───────────────────────────────────────────────────────

  private async Task ObtainTokenAsync(CancellationToken cancellationToken)
  {
    string? adminKey = _configuration["PowerApi:AdminKey"];
    string? serviceName = _configuration["PowerApi:ServiceName"];

    if (string.IsNullOrWhiteSpace(adminKey))
    {
      _logger.LogError("PowerApi:AdminKey is not configured. Power API authentication will not work.");
      return;
    }

    serviceName = string.IsNullOrWhiteSpace(serviceName) ? "boardom-frontend" : serviceName;

    var body = new
    {
      service = serviceName,
      audience = "power-pi",
      scopes = new[] { "power-table:read", "power-table:write" }
    };

    try
    {
      var client = CreateAdminClient(adminKey);
      var json = JsonSerializer.Serialize(body);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await client.PostAsync("/admin/tokens", content, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to obtain Power API token. Status: {Status}, Body: {Body}",
            response.StatusCode, errorBody);
        return;
      }

      await ParseTokenResponseAsync(response, cancellationToken);
      _logger.LogInformation("Power API token obtained. Expires: {ExpiresAt}, JTI: {Jti}", _expiresAt, _jti);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception while obtaining Power API token");
    }
  }

  private async Task RotateTokenAsync(CancellationToken cancellationToken)
  {
    string? adminKey = _configuration["PowerApi:AdminKey"];
    string? serviceName = _configuration["PowerApi:ServiceName"];

    if (string.IsNullOrWhiteSpace(adminKey))
    {
      _logger.LogError("PowerApi:AdminKey is not configured. Cannot rotate Power API token.");
      return;
    }

    serviceName = string.IsNullOrWhiteSpace(serviceName) ? "boardom-frontend" : serviceName;

    var body = new
    {
      service = serviceName,
      audience = "power-pi",
      scopes = new[] { "power-table:read", "power-table:write" },
      old_jti = _jti
    };

    try
    {
      var client = CreateAdminClient(adminKey);
      var json = JsonSerializer.Serialize(body);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await client.PostAsync("/admin/tokens/rotate", content, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to rotate Power API token. Status: {Status}, Body: {Body}",
            response.StatusCode, errorBody);
        // Fall back to obtaining a fresh token
        await ObtainTokenAsync(cancellationToken);
        return;
      }

      await ParseTokenResponseAsync(response, cancellationToken);
      _logger.LogInformation("Power API token rotated. Expires: {ExpiresAt}, JTI: {Jti}", _expiresAt, _jti);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception while rotating Power API token");
    }
  }

  private async Task ParseTokenResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
  {
    var raw = await response.Content.ReadAsStringAsync(cancellationToken);
    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(raw);

    if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.Token))
    {
      _logger.LogError("Power API returned an empty or unparseable token response");
      return;
    }

    _token = tokenResponse.Token;
    _jti = tokenResponse.Jti;
    _expiresAt = tokenResponse.ExpiresAt != default
        ? tokenResponse.ExpiresAt
        : DateTimeOffset.UtcNow.AddHours(24);
  }

  private void ScheduleRotation()
  {
    _rotationTimer?.Dispose();

    if (string.IsNullOrEmpty(_token))
      return;

    var rotateAt = _expiresAt - RotationLeadTime;
    var delay = rotateAt - DateTimeOffset.UtcNow;

    if (delay <= TimeSpan.Zero)
      delay = TimeSpan.FromMinutes(1);

    _rotationTimer = new Timer(
        async _ =>
        {
          _logger.LogInformation("Scheduled Power API token rotation starting.");
          await ForceRotateAsync();
        },
        state: null,
        dueTime: delay,
        period: Timeout.InfiniteTimeSpan);

    _logger.LogInformation("Power API token rotation scheduled in {Delay:hh\\:mm\\:ss}.", delay);
  }

  private HttpClient CreateAdminClient(string adminKey)
  {
    var client = _httpClientFactory.CreateClient("PowerApi");
    // Replace any existing default Authorization header with the admin key header
    client.DefaultRequestHeaders.Remove("X-Admin-Key");
    client.DefaultRequestHeaders.Add("X-Admin-Key", adminKey);
    client.DefaultRequestHeaders.Authorization = null;
    return client;
  }

  // ── DTOs ──────────────────────────────────────────────────────────────────

  private sealed class TokenResponse
  {
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("jti")]
    public string? Jti { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
  }
}
