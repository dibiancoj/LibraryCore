using LibraryCore.Core.ExtensionMethods;

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

    [Fact]
    public void PeekCharacterTest()
    {
        using var reader = new StringReader("abc");

        Assert.Equal('a', reader.PeekCharacter());

        _ = reader.Read();

        Assert.Equal('b', reader.PeekCharacter());

        _ = reader.Read();

        Assert.Equal('c', reader.PeekCharacter());
    }
}
