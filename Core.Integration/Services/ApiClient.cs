using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using Core.Integration.Interfaces;
using Core.Integration.Models;
using System.Text;
using System.Text.Json;

namespace Core.Integration.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<T>> SendRequestAsync<T>(ApiRequest request)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(request.Method, request.Url)
            {
                Content = request.Body != null ? new StringContent(JsonSerializer.Serialize(request.Body), Encoding.UTF8, "application/json") : null
            };

            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(httpRequest);
            var responseData = await response.Content.ReadAsStringAsync();

            var deserializedData = response.IsSuccessStatusCode ? JsonSerializer.Deserialize<T>(responseData) : default;

            if (response.IsSuccessStatusCode && deserializedData == null)
            {
                throw new CustomInvalidOperationException("Deserialization returned null for a successful response.");
            }

            return new ApiResponse<T>
            {
                IsSuccess = response.IsSuccessStatusCode,
                Data = deserializedData,
                ErrorMessage = response.IsSuccessStatusCode ? null : responseData
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }
}