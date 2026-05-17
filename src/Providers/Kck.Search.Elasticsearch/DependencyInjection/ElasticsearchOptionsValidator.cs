using Microsoft.Extensions.Options;

namespace Kck.Search.Elasticsearch.DependencyInjection;

public sealed class ElasticsearchOptionsValidator : IValidateOptions<ElasticsearchOptions>
{
    public ValidateOptionsResult Validate(string? name, ElasticsearchOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            errors.Add(
                """
                  • ConnectionString: boş veya null
                    → Fix: opt.ConnectionString = "https://localhost:9200"
                """);
        }
        else if (!Uri.TryCreate(options.ConnectionString, UriKind.Absolute, out var uri)
                 || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            errors.Add(
                $"""
                  • ConnectionString: "{options.ConnectionString}" geçerli bir HTTP/HTTPS URI değil
                    → Fix: opt.ConnectionString = "https://localhost:9200"
                """);
        }

        if (options.NumberOfShards < 1)
            errors.Add(
                $"""
                  • NumberOfShards: {options.NumberOfShards} (en az 1 olmalı)
                    → Fix: opt.NumberOfShards = 1
                """);

        if (options.NumberOfReplicas < 0)
            errors.Add(
                $"""
                  • NumberOfReplicas: {options.NumberOfReplicas} (0 veya daha büyük olmalı)
                    → Fix: opt.NumberOfReplicas = 0   // tek node için; prod: 1+
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Search.Elasticsearch] ElasticsearchOptions geçersiz:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/search.md
            """);
    }
}
