namespace Kck.Security.Argon2;

/// <summary>Argon2id hashing parameters (OWASP recommended defaults).</summary>
public sealed class Argon2Options
{
    /// <summary>Degree of parallelism (number of threads). Default: 1.</summary>
    public int DegreeOfParallelism { get; set; } = 1;

    /// <summary>Memory size in KB. Default: 65536 (64 MB).</summary>
    public int MemorySize { get; set; } = 65536;

    /// <summary>Number of iterations. Default: 3.</summary>
    public int Iterations { get; set; } = 3;

    /// <summary>Length of the derived hash in bytes. Default: 32.</summary>
    public int HashLength { get; set; } = 32;

    /// <summary>Length of the salt in bytes. Default: 16.</summary>
    public int SaltLength { get; set; } = 16;
}
