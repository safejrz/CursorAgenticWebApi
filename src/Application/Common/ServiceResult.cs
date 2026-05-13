namespace CursorAgenticWebApi.Application.Common;

public abstract record ServiceResult<T>
{
    private ServiceResult() { }

    public sealed record Success(T Value) : ServiceResult<T>;

    public sealed record Failure(string Code, string Message) : ServiceResult<T>;

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, string, TResult> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Value),
            Failure f => onFailure(f.Code, f.Message),
            _ => throw new InvalidOperationException(),
        };
}
