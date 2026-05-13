namespace CursorAgenticWebApi.Application.Abstractions;

public interface IDatabaseBootstrapper
{
    Task EnsureDatabaseAsync(CancellationToken cancellationToken = default);
}
