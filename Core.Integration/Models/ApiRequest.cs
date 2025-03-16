namespace Core.Integration.Models;

public class ApiRequest
{
    public string Url { get; set; } = default!;
    public HttpMethod Method { get; set; } = default!;
    public object Body { get; set; } = default!;
    public Dictionary<string, string> Headers { get; set; } = new();
}