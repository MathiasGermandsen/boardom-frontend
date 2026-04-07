using Microsoft.AspNetCore.Authentication;

namespace Boardom.Services;

public class ApiTokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiTokenService (IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task <string?> GetAccessTokenAsync()
    {
        return await _httpContextAccessor.HttpContext!
            .GetTokenAsync("access_token");
    }
}