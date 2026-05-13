namespace CursorAgenticWebApi.Application.Abstractions;

public interface IDataSeeder
{
    Task SeedDemoDataIfEmptyAsync(CancellationToken cancellationToken = default);
}
