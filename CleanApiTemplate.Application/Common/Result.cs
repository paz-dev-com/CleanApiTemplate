namespace CleanApiTemplate.Application.Common;

/// <summary>
/// Generic result wrapper for operation outcomes
/// Provides a consistent way to return success/failure with errors
/// </summary>
/// <typeparam name="T">Type of the result data</typeparam>
public class Result<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Indicates if the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The result data (only populated on success)
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// Error message (only populated on failure)
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Collection of validation errors
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }

    private Result(bool isSuccess, T? data, string? error, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Create a successful result with data
    /// </summary>
    public static Result<T> Success(T data) => new(true, data, null);

    /// <summary>
    /// Create a failure result with error message
    /// </summary>
    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Create a failure result with validation errors
    /// </summary>
    public static Result<T> ValidationFailure(Dictionary<string, string[]> validationErrors) =>
        new(false, default, "Validation failed", validationErrors);
}

/// <summary>
/// Result wrapper without data payload
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; private set; }
    public Dictionary<string, string[]>? ValidationErrors { get; private set; }

    private Result(bool isSuccess, string? error, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);

    public static Result ValidationFailure(Dictionary<string, string[]> validationErrors) =>
        new(false, "Validation failed", validationErrors);
}
