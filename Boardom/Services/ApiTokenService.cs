using Microsoft.AspNetCore.Authentication;

namespace Boardom.Services;

public class ApiTokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiTokenService> _logger;

    public ApiTokenService(IHttpContextAccessor httpContextAccessor, ILogger<ApiTokenService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            _logger.LogError("HttpContext: null");
            return null;
        }

        string? accessToken = await httpContext.GetTokenAsync("access_token");
        string? idToken = await httpContext.GetTokenAsync("id_token");

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Accesstoken: null");
        }

        if (string.IsNullOrEmpty(idToken))
        {
            _logger.LogError("idToken: null");
        }

        _logger.LogInformation($"access_token parts: {accessToken?.Split('.')?.Length ?? 0}");
        _logger.LogInformation($"id_token parts: {idToken?.Split('.')?.Length ?? 0}");
        _logger.LogInformation($"access_token value: {accessToken?[..50] ?? "NULL"}");

        return accessToken;
    }

    public async Task AttachToken(HttpClient client)
    {
        string? token = await GetAccessTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = null;
            return;
        }

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}