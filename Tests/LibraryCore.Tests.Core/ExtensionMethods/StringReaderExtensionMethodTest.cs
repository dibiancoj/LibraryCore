using LibraryCore.Core.ExtensionMethods;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class StringReaderExtensionMethodTest
{
    [Fact]
    public void HasMoreCharacterTest()
    {
        using var reader = new StringReader("ab");

        Assert.True(reader.HasMoreCharacters());
        reader.Read();
        Assert.True(reader.HasMoreCharacters());
        reader.Read();
        Assert.False(reader.HasMoreCharacters());
    }

    [Fact]
    public void ReadCharacterTest()
    {
        using var reader = new StringReader("a");

        Assert.Equal('a', reader.ReadCharacter());
    }
}
