using System.Net;
using System.Net.Http.Headers;

namespace Boardom.Services;

public class PowerApiAuthHandler : DelegatingHandler
{
  private readonly PowerApiTokenService _tokenService;
  private readonly ILogger<PowerApiAuthHandler> _logger;

  public PowerApiAuthHandler(PowerApiTokenService tokenService, ILogger<PowerApiAuthHandler> logger)
  {
    _tokenService = tokenService;
    _logger = logger;
  }

  protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
  {
    string? token = await _tokenService.GetTokenAsync(cancellationToken);

    if (!string.IsNullOrEmpty(token))
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    else
      _logger.LogWarning("No Power API token available; sending request without Authorization header.");

    var response = await base.SendAsync(request, cancellationToken);

    // On 401, force-rotate and retry once
    if (response.StatusCode == HttpStatusCode.Unauthorized)
    {
      _logger.LogWarning("Power API returned 401. Force-rotating token and retrying.");
      response.Dispose();

      await _tokenService.ForceRotateAsync(cancellationToken);

      token = await _tokenService.GetTokenAsync(cancellationToken);
      if (!string.IsNullOrEmpty(token))
      {
        // Clone the request because HttpRequestMessage cannot be sent twice
        var retry = await CloneRequestAsync(request, cancellationToken);
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        response = await base.SendAsync(retry, cancellationToken);
      }
    }

    return response;
  }

  private static async Task<HttpRequestMessage> CloneRequestAsync(
      HttpRequestMessage original,
      CancellationToken cancellationToken)
  {
    var clone = new HttpRequestMessage(original.Method, original.RequestUri);

    foreach (var header in original.Headers)
      clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

    if (original.Content is not null)
    {
      var bytes = await original.Content.ReadAsByteArrayAsync(cancellationToken);
      clone.Content = new ByteArrayContent(bytes);

      foreach (var header in original.Content.Headers)
        clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
    }

    return clone;
  }
}
