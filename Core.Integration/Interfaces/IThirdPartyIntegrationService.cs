using Core.Integration.Models;

namespace Core.Integration.Interfaces;

public interface IThirdPartyIntegrationService
{
    Task<ApiResponse<string>> PerformIntegrationAsync(string data);
}