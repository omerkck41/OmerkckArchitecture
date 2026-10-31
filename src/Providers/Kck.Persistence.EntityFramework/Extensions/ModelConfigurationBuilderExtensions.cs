using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kck.Persistence.EntityFramework.Extensions;

/// <summary>Model conventions for <see cref="DbContext.ConfigureConventions"/>.</summary>
public static class ModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Stores every <see cref="DateTime"/> and <see cref="Nullable{DateTime}"/> property as UTC and reads it back with
    /// <see cref="DateTimeKind.Utc"/>. SQLite and SQL Server <c>datetime2</c> otherwise return
    /// <see cref="DateTimeKind.Unspecified"/>, so audit dates (written in UTC) serialize without the "Z" suffix. A local
    /// value is converted to UTC when written; an unspecified one is taken as UTC.
    /// </summary>
    /// <example>
    /// <code>
    /// protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
    ///     configurationBuilder.UseUtcDateTimes();
    /// </code>
    /// </example>
    public static ModelConfigurationBuilder UseUtcDateTimes(this ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
        return configurationBuilder;
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
