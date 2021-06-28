using LibraryCore.Core.ExtensionMethods;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods
{
    public class DictionaryExtensionMethodTest
    {

        [Fact]
        public void GetOrAdd()
        {
            var dictionary = new Dictionary<string, string>();

            Assert.Equal("item1", dictionary.GetOrAdd("key1", () => "item1"));

            dictionary.Add("key2", "item2");

            Assert.Equal("item2", dictionary.GetOrAdd("key2", () => "Shouldnt Come Back"));
        }

        [Fact]
        public async Task GetOrAddAsync()
        {
            var dictionary = new Dictionary<string, string>();

            Assert.Equal("item1", await dictionary.GetOrAddAsync("key1", async () => await Task.FromResult("item1")));

            dictionary.Add("key2", "item2");

            Assert.Equal("item2", await dictionary.GetOrAddAsync("key2", async () => await Task.FromResult("ShouldntComeBack")));
        }

    }
}
