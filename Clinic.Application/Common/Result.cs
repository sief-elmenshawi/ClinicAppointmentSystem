namespace Clinic.Application.Common;

public enum ErrorType
{
    BadRequest,
    NotFound,
    Forbidden,
    Conflict
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null, ErrorType.BadRequest);
    public static Result<T> Failure(string error, ErrorType errorType = ErrorType.BadRequest)
        => new(false, default, error, errorType);
}