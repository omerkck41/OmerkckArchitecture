namespace Core.Integration.Models;

public class ApiRequest
{
    public string Url { get; set; }
    public HttpMethod Method { get; set; }
    public object Body { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}