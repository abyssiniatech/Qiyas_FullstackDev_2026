using Microsoft.AspNetCore.Http;

namespace TmsApi.Api.RateLimiting;

public class ApiKeyResolver
{
    private readonly IConfiguration _configuration;

    public ApiKeyResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }



    public ApiKeyTier Resolve(HttpContext context)
    {
        // Read API key from request header
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
        {
            return ApiKeyTier.Free;
        }


        var key = apiKey.ToString();


        // Get configured API keys
        var apiKeys = _configuration
            .GetSection("ApiKeys")
            .GetChildren();


        foreach (var item in apiKeys)
        {
            if (item["Key"] == key)
            {
                if (Enum.TryParse<ApiKeyTier>(
                    item["Tier"],
                    true,
                    out var tier))
                {
                    return tier;
                }
            }
        }


        return ApiKeyTier.Free;
    }
}