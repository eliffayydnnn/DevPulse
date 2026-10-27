namespace DevPulse.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Details { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> Fail(string errorMessage, object? details = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        Details = details
    };
}
