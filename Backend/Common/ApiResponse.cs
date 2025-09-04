namespace MonBackend.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    
    public static ApiResponse<T> SuccessResponse(T data, string message = "")
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }
    
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors ?? new() };
    }
}