using Microsoft.Extensions.Logging;

namespace CleanApiTemplate.Data.Seeders;

/// <summary>
/// Provides high-performance logging helpers for seeders using LoggerMessage.Define
/// Centralizes common logging patterns to avoid repetitive static declarations
/// </summary>
public static class SeederLoggerHelper
{
    private static readonly Action<ILogger, string, string, Exception?> _logSeederStart =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, "SeederStart"),
            "Seeding default {SeederType} {EntityType}...");

    private static readonly Action<ILogger, string, string, Exception?> _logSeederSkip =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(2, "SeederSkip"),
            "{SeederType} {EntityType} already exist, skipping seed");

    private static readonly Action<ILogger, string, int, Exception?> _logSeederComplete =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(3, "SeederComplete"),
            "Successfully seeded {Count} {EntityType}");

    private static readonly Action<ILogger, string, Exception?> _logSeederError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(4, "SeederError"),
            "Failed to seed {EntityType}");

    private static readonly Action<ILogger, string, string, Exception?> _logSeederCustomInfo =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(5, "SeederCustomInfo"),
            "{SeederType}: {Message}");

    /// <summary>
    /// Logs the start of a seeding operation
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="entityType">The type of entity being seeded (e.g., "categories", "users")</param>
    public static void LogSeederStart<T>(this ILogger<T> logger, string entityType)
    {
        _logSeederStart(logger, typeof(T).Name, entityType, null);
    }

    /// <summary>
    /// Logs that seeding was skipped because data already exists
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="entityType">The type of entity that already exists</param>
    public static void LogSeederSkip<T>(this ILogger<T> logger, string entityType)
    {
        _logSeederSkip(logger, typeof(T).Name, entityType, null);
    }

    /// <summary>
    /// Logs successful completion of seeding with count
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="entityType">The type of entity that was seeded</param>
    /// <param name="count">Number of entities seeded</param>
    public static void LogSeederComplete<T>(this ILogger<T> logger, string entityType, int count)
    {
        _logSeederComplete(logger, entityType, count, null);
    }

    /// <summary>
    /// Logs a seeding error
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="entityType">The type of entity that failed to seed</param>
    /// <param name="exception">The exception that occurred</param>
    public static void LogSeederError<T>(this ILogger<T> logger, string entityType, Exception exception)
    {
        _logSeederError(logger, entityType, exception);
    }

    /// <summary>
    /// Logs a custom informational message for seeders
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="message">The custom message to log</param>
    public static void LogSeederInfo<T>(this ILogger<T> logger, string message)
    {
        _logSeederCustomInfo(logger, typeof(T).Name, message, null);
    }
}
