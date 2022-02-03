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

    [Fact]
    public void EatXNumberOfCharactersTest()
    {
        using var reader = new StringReader("abcd");

        reader.EatXNumberOfCharacters(1);

        Assert.Equal('b', reader.PeekCharacter());

        reader.EatXNumberOfCharacters(2);

        Assert.Equal('d', reader.PeekCharacter());

        //try when we are at the end of file
        reader.EatXNumberOfCharacters(10);
    }
}
