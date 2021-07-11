using LibraryCore.AspNet.SessionState;
using LibraryCore.Tests.AspNet.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCore.Tests.AspNet.SessionState
{
    public class DistributedSessionStateServiceTest
    {

        public DistributedSessionStateServiceTest()
        {
            SessionStateServiceToUse = new DistributedSessionStateService(FullMockSessionState.BuildContextWithSession().MockContextAccessor.Object, Array.Empty<System.Text.Json.Serialization.JsonConverter>());
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

            Assert.False((await SessionStateServiceToUse.TryGetObjectAsync<Guid>(key)).ItemFoundInSession);
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

            //back with a try get
            var result = await SessionStateServiceToUse.TryGetObjectAsync<BaseClass>(key, true);

            Assert.True(result.ItemFoundInSession);

            Assert.Equal(24, result.ItemInSession.Id);

            //use the get
            Assert.Equal(24, (await SessionStateServiceToUse.GetObjectAsync<BaseClass>(key, true)).Id);
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
}
