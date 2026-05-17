namespace Kck.FileStorage.AzureBlob;

/// <summary>
/// Configuration for <see cref="AzureBlobStorageService"/>.
/// Provide either <see cref="ConnectionString"/> or <see cref="AccountName"/> (Managed Identity).
/// </summary>
public sealed class AzureBlobOptions
{
    /// <summary>
    /// Azure Storage connection string.
    /// Mutually exclusive with <see cref="AccountName"/>; prefer <see cref="AccountName"/> for production.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Azure Storage account name used with DefaultAzureCredential (Managed Identity).
    /// URI format: <c>https://&lt;account&gt;.blob.core.windows.net</c>.
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// Default container name. Used when the upload path does not include a container prefix.
    /// </summary>
    public required string ContainerName { get; set; }

    /// <summary>
    /// When <see langword="true"/>, creates the container if it does not exist.
    /// Default: <see langword="true"/>.
    /// </summary>
    public bool CreateContainerIfNotExists { get; set; } = true;
}
