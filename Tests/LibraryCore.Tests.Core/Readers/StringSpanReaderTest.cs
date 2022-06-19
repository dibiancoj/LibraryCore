using LibraryCore.Core.Readers;

namespace LibraryCore.Tests.Core.Readers;

public class StringSpanReaderTest
{
    [Fact]
    public void ReadCorrectly()
    {
        var reader = new StringSpanReader("Abc");

        Assert.Equal('A', reader.ReadCharacter());
        Assert.Equal('b', reader.ReadCharacter());
        Assert.Equal('c', reader.ReadCharacter());
    }

    [Fact]
    public void HasMoreCharacters()
    {
        var reader = new StringSpanReader("bc");

        Assert.True(reader.HasMoreCharacters());
        reader.ReadCharacter(); //should be at c
        Assert.True(reader.HasMoreCharacters());
        reader.ReadCharacter(); //should be after c
        Assert.False(reader.HasMoreCharacters());
    }

    [Fact]
    public void PeekCorrectly()
    {
        var reader = new StringSpanReader("Abc");

        Assert.Equal('A', reader.PeekCharacter());
        Assert.Equal('A', reader.ReadCharacter());
        Assert.Equal('b', reader.PeekCharacter());
        Assert.Equal('b', reader.ReadCharacter());
        Assert.Equal('c', reader.PeekCharacter());
        Assert.Equal('c', reader.ReadCharacter());
        Assert.Null(reader.PeekCharacter()); //shouldn't throw as we check if its the end of the string
    }

    [Fact]
    public void PeekMultipleWhenNotEnoughCharacters() => Assert.Equal("Abc", new StringSpanReader("Abc").PeekCharacter(5));

    [Fact]
    public void PeekMultipleWhenAtTheEnd() => Assert.Null(new StringSpanReader("").PeekCharacter(1));

    [Fact]
    public void PeekMultipleWhenHaveCharactersLeftOverAfterPeek() => Assert.Equal("Ab", new StringSpanReader("Abcdef").PeekCharacter(2));

    [Fact]
    public void ReadMultipleWhenNotEnoughCharacters() => Assert.Equal("Abc", new StringSpanReader("Abc").ReadCharacter(5));

    [Fact]
    public void ReadMultipleWhenAtTheEnd() => Assert.Null(new StringSpanReader("").ReadCharacter(1));

    [Fact]
    public void ReadMultipleWhenHaveCharactersLeftOverAfterPeek()
    {
        var reader = new StringSpanReader("Abcdef");

        Assert.Equal("Ab", reader.ReadCharacter(2));

        //now make sure we are up to c
        Assert.Equal('c', reader.PeekCharacter());

        //read some more
        Assert.Equal("cd", reader.ReadCharacter(2));
    }

    [Fact]
    public void EnsurePassingStructToMethodsWork()
    {
        var reader = new StringSpanReader("Abcdef");

        Assert.Equal('A', reader.ReadCharacter());

        PassStruct1(ref reader);
        Assert.Equal('d', reader.ReadCharacter());
    }

    private static void PassStruct1(ref StringSpanReader reader)
    {
        Assert.Equal('b', reader.ReadCharacter());
        PassStruct2(ref reader);
    }

    private static void PassStruct2(ref StringSpanReader reader) => Assert.Equal('c', reader.ReadCharacter());

    [Fact]
    public void ReadUntilCharacterWhenFound()
    {
        var reader = new StringSpanReader("1 == 1 && 2 == 2");

        var result = reader.ReadUntilCharacter("&&", StringComparison.OrdinalIgnoreCase);

        Assert.Equal("1 == 1 ", result);
        Assert.Equal('&', reader.PeekCharacter());
    }

    [Fact]
    public void ReadUntilCharacterWhenFoundAndInMiddleOfString()
    {
        var reader = new StringSpanReader("1 == 1 && 2 == 2");

        //skip until the second ==
        Assert.Equal("1 ==", reader.ReadCharacter(4));

        var result = reader.ReadUntilCharacter("&&", StringComparison.OrdinalIgnoreCase);

        Assert.Equal(" 1 ", result);
    }

    [Fact]
    public void ReadUntilCharacterWhenNotFound()
    {
        var reader = new StringSpanReader("1 == 1 && 2 == 2");

        Assert.Null(reader.ReadUntilCharacter("abc", StringComparison.OrdinalIgnoreCase));
    }

}
