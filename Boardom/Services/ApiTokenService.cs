using Microsoft.AspNetCore.Authentication;

namespace Boardom.Services;

public class ApiTokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiTokenService (IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return null;
        }

        return await httpContext.GetTokenAsync("access_token");
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