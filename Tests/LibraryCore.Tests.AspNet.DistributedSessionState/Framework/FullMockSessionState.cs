using Microsoft.AspNetCore.Http;

namespace LibraryCore.Tests.AspNet.DistributedSessionState.Framework;

public record MockWithSessionResult(Mock<IHttpContextAccessor> MockContextAccessor, Mock<HttpContext> MockContext, Mock<ISession> FullSessionStateMock);

public class FullMockSessionState : ISession
{
    private Dictionary<string, byte[]> InternalSessionStateStorage { get; } = [];

    public bool IsAvailable => true;

    public string Id => Guid.NewGuid().ToString();

    public IEnumerable<string> Keys => InternalSessionStateStorage.Keys;

    public virtual void Clear() => InternalSessionStateStorage.Clear();

    public virtual Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual void Remove(string key) => InternalSessionStateStorage.Remove(key);

    public virtual void Set(string key, byte[] value) => InternalSessionStateStorage[key] = value;

    public virtual bool TryGetValue(string key, out byte[] value) => InternalSessionStateStorage.TryGetValue(key, out value!);

    public static MockWithSessionResult BuildContextWithSession()
    {
        var mockHttpContext = new Mock<HttpContext>();
        var mockIHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var fullSessionStateMock = new Mock<FullMockSessionState>() { CallBase = true }.As<ISession>();

        mockIHttpContextAccessor.Setup(x => x.HttpContext)
            .Returns(mockHttpContext.Object);

        mockHttpContext.Setup(x => x.Session)
            .Returns(fullSessionStateMock.Object);

        return new(mockIHttpContextAccessor, mockHttpContext, fullSessionStateMock);
    }
}