using Core.Integration.Interfaces;
using Core.Integration.Models;
using Microsoft.Extensions.Options;

namespace Core.Integration.Services;

public class ThirdPartyIntegrationService : IThirdPartyIntegrationService
{
    private readonly IApiClient _apiClient;
    private readonly ThirdPartyOptions _options;

    public ThirdPartyIntegrationService(IApiClient apiClient, IOptions<ThirdPartyOptions> options)
    {
        _apiClient = apiClient;
        _options = options.Value;
    }

    public async Task<ApiResponse<string>> PerformIntegrationAsync(string data)
    {
        var request = new ApiRequest
        {
            Url = _options.Endpoint,
            Method = HttpMethod.Post,
            Body = new { data },
            Headers = new Dictionary<string, string> { { "Authorization", $"Bearer {_options.ApiKey}" } }
        };

        return await _apiClient.SendRequestAsync<string>(request);
    }
}