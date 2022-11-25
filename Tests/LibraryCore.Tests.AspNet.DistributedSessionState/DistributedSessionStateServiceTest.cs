using LibraryCore.AspNet.DistributedSessionState;
using LibraryCore.Tests.AspNet.DistributedSessionState.Framework;

namespace LibraryCore.Tests.AspNet.DistributedSessionState;

public class DistributedSessionStateServiceTest
{
    public DistributedSessionStateServiceTest()
    {
        SessionStateServiceToUse = new DistributedSessionStateService(FullMockSessionState.BuildContextWithSession().MockContextAccessor.Object);
    }

    private DistributedSessionStateService SessionStateServiceToUse { get; }

    [Fact]
    public async Task HasKeyInSessionTest()
    {
        var key = nameof(HasKeyInSessionTest);

        Assert.False(await SessionStateServiceToUse.HasKeyInSessionAsync(key));

        await SessionStateServiceToUse.SetObjectAsync(key, Guid.NewGuid());

        Assert.True(await SessionStateServiceToUse.HasKeyInSessionAsync(key));
    }

    [Fact]
    public async Task RemoveSessionItem()
    {
        var key = nameof(HasKeyInSessionTest);

        await SessionStateServiceToUse.SetObjectAsync(key, Guid.NewGuid());

        await SessionStateServiceToUse.RemoveObjectAsync(key);

        Assert.False((await SessionStateServiceToUse.TryGetObjectAsync<Guid>(key)).GetItemIfFoundInSession(out _));
    }

    [Fact]
    public async Task ClearAllSessionObjectsForThisUser()
    {
        var key = nameof(ClearAllSessionObjectsForThisUser);

        await SessionStateServiceToUse.SetObjectAsync(key, Guid.NewGuid());

        Assert.True(await SessionStateServiceToUse.HasKeyInSessionAsync(key));

        await SessionStateServiceToUse.ClearAllSessionObjectsForThisUserAsync();

        Assert.False(await SessionStateServiceToUse.HasKeyInSessionAsync(key));
    }

    [Fact]
    public async Task GetOrSetTest()
    {
        var key = nameof(GetOrSetTest);

        Assert.Equal("Create From Source", await SessionStateServiceToUse.GetOrSetAsync(key, () => Task.FromResult("Create From Source")));

        //grab it again...it should pull from session
        Assert.Equal("Create From Source", await SessionStateServiceToUse.GetOrSetAsync(key, () => Task.FromResult("Shouldn't be called")));
    }

    [Fact]
    public async Task SerializeAndDeserializeAbstractClasses()
    {
        var key = nameof(SerializeAndDeserializeAbstractClasses);
        var model = new DerivedClass { Id = 24 };

        await SessionStateServiceToUse.SetObjectAsync(key, model, true);

        var result = await SessionStateServiceToUse.TryGetObjectAsync<BaseClass>(key, true);

        Assert.True(result.GetItemIfFoundInSession(out var itemFoundInSession));
        Assert.Equal(24, itemFoundInSession!.Id);
        Assert.Equal(24, (await SessionStateServiceToUse.GetObjectAsync<BaseClass>(key, true))!.Id);
    }

    [Fact]
    public async Task SerializeAndDeserializeInterface()
    {
        var keyA = nameof(SerializeAndDeserializeInterface) + "_a";
        var keyB = nameof(SerializeAndDeserializeInterface) + "_B";
        var aModel = new SessionTestA();
        var bModel = new SessionTestB();

        await SessionStateServiceToUse.SetObjectAsync(keyA, aModel, true);
        await SessionStateServiceToUse.SetObjectAsync(keyB, bModel, true);

        var resultA = await SessionStateServiceToUse.TryGetObjectAsync<ISessionTest>(keyA, true);
        var resultB = await SessionStateServiceToUse.TryGetObjectAsync<ISessionTest>(keyB, true);

        Assert.True(resultA.FoundInSession);
        Assert.True(resultB.FoundInSession);
        Assert.Equal(24, resultA.ItemInSessionIfFound!.Result);
        Assert.Equal(25, resultB.ItemInSessionIfFound!.Result);
    }

    [Fact]
    public async Task SerializeAndDeserializeInterfaceWithNull()
    {
        var key = nameof(SerializeAndDeserializeInterfaceWithNull);

        SessionTestA? model = null;

        await SessionStateServiceToUse.SetObjectAsync(key, model, true);

        var result = await SessionStateServiceToUse.TryGetObjectAsync<ISessionTest>(key, true);

        Assert.True(result.FoundInSession);
        Assert.Null(result.ItemInSessionIfFound);
    }

    public interface ISessionTest
    {
        public int Result { get; }
    }

    public class SessionTestA : ISessionTest
    {
        public int Result => 24;
    }

    public class SessionTestB : ISessionTest
    {
        public int Result => 25;
    }

    public abstract class BaseClass
    {
        public abstract int Id { get; set; }
    }

    public class DerivedClass : BaseClass
    {
        public override int Id { get; set; }
    }

    [Fact]
    public async Task KeysIsCorrect()
    {
        Assert.Empty(await SessionStateServiceToUse.SessionItemKeysAsync());

        await SessionStateServiceToUse.SetObjectAsync("key1", "test1");

        Assert.Single(await SessionStateServiceToUse.SessionItemKeysAsync());
        Assert.Equal("key1", (await SessionStateServiceToUse.SessionItemKeysAsync()).Single());

        await SessionStateServiceToUse.SetObjectAsync("key2", "test2");

        var keys = await SessionStateServiceToUse.SessionItemKeysAsync();

        Assert.Equal(2, keys.Count());
        Assert.Contains(keys, x => x == "key1");
        Assert.Contains(keys, x => x == "key2");
    }

}
