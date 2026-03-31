namespace TaskManagerApp.Services;

public class ApiException : Exception
{
    public int? StatusCode { get; }

    public ApiException(string message, Exception? inner = null, int? statusCode = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }
}
