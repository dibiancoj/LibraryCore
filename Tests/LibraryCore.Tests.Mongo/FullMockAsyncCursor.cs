using MongoDB.Driver;

namespace LibraryCore.Tests.Mongo;

public class MockAsyncCursor<T>(IEnumerable<T> items) : IAsyncCursor<T>
{
    private bool called = false;

    public IEnumerable<T> Current { get; } = items ?? [];

    public bool MoveNext(CancellationToken cancellationToken = new CancellationToken())
    {
        return !called && (called = true);
    }

    public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(MoveNext(cancellationToken));
    }

    public void Dispose()
    {
    }
}
