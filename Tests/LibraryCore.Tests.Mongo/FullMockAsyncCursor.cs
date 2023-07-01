using MongoDB.Driver;

namespace LibraryCore.Tests.Mongo;

public class MockAsyncCursor<T> : IAsyncCursor<T>
{
    private bool called = false;

    public MockAsyncCursor(IEnumerable<T> items)
    {
        Current = items ?? Enumerable.Empty<T>();
    }

    public IEnumerable<T> Current { get; }

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
