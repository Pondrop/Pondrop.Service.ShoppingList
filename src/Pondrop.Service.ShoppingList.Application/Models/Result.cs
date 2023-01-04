namespace Pondrop.Service.ShoppingList.Application.Models;

public class Result<T>
{
    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Error(Exception exception, string message) => new Result<T>(exception, message);
    public static Result<T> Error(Exception exception) => new Result<T>(exception, exception.Message);
    public static Result<T> Error(string message) => new Result<T>(null, message);

    public T? Value { get; }
    public Exception? Exception { get; }
    public string ErrorMessage { get; }
    public bool IsSuccess { get; }

    private Result(T value)
    {
        Value = value;
        ErrorMessage = string.Empty;
        IsSuccess = true;
    }

    private Result(Exception? exception, string errorMessage)
    {
        Exception = exception;
        ErrorMessage = errorMessage;
        IsSuccess = false;
    }

    public void Match(Action<T?> onSuccess, Action<Exception?, string> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value);
        else
            onFailure(Exception, ErrorMessage);
    }

    public Task MatchAsync(Func<T?, Task> onSuccess, Func<Exception?, string, Task> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Exception, ErrorMessage);

    public TResult Match<TResult>(Func<T?, TResult> onSuccess, Func<Exception?, string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Exception, ErrorMessage);

    public Task<TResult> MatchAsync<TResult>(Func<T?, Task<TResult>> onSuccess, Func<Exception?, string, Task<TResult>> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Exception, ErrorMessage);
}
