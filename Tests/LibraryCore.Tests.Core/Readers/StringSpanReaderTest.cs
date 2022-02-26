using LibraryCore.Core.Readers;

namespace LibraryCore.Tests.Core.Readers;

public class StringSpanReaderTest
{
    [Fact]
    public void ReadCorrectly()
    {
        var reader = new StringSpanReader("Abc");

        Assert.Equal('A', reader.Read());
        Assert.Equal('b', reader.Read());
        Assert.Equal('c', reader.Read());
    }

    [Fact]
    public void HasMoreCharacters()
    {
        var reader = new StringSpanReader("bc");

        Assert.True(reader.HasMoreCharacters());
        reader.Read(); //should be at c
        Assert.True(reader.HasMoreCharacters());
        reader.Read(); //should be after c
        Assert.False(reader.HasMoreCharacters());
    }

    [Fact]
    public void PeekCorrectly()
    {
        var reader = new StringSpanReader("Abc");

        Assert.Equal('A', reader.Peek());
        Assert.Equal('A', reader.Read());
        Assert.Equal('b', reader.Peek());
        Assert.Equal('b', reader.Read());
        Assert.Equal('c', reader.Peek());
        Assert.Equal('c', reader.Read());
        Assert.Null(reader.Peek()); //shouldn't throw as we check if its the end of the string
    }

    [Fact]
    public void PeekMultipleWhenNotEnoughCharacters() => Assert.Equal("Abc", new StringSpanReader("Abc").Peek(5));

    [Fact]
    public void PeekMultipleWhenAtTheEnd() => Assert.Null(new StringSpanReader("").Peek(1));

    [Fact]
    public void PeekMultipleWhenHaveCharactersLeftOverAfterPeek() => Assert.Equal("Ab", new StringSpanReader("Abcdef").Peek(2));

    [Fact]
    public void ReadMultipleWhenNotEnoughCharacters() => Assert.Equal("Abc", new StringSpanReader("Abc").Read(5));

    [Fact]
    public void ReadMultipleWhenAtTheEnd() => Assert.Null(new StringSpanReader("").Read(1));

    [Fact]
    public void ReadMultipleWhenHaveCharactersLeftOverAfterPeek()
    {
        var reader = new StringSpanReader("Abcdef");

        Assert.Equal("Ab", reader.Read(2));

        //now make sure we are up to c
        Assert.Equal('c', reader.Peek());

        //read some more
        Assert.Equal("cd", reader.Read(2));
    }

    [Fact]
    public void EnsurePassingStructToMethodsWork()
    {
        var reader = new StringSpanReader("Abcdef");

        Assert.Equal('A', reader.Read());

        PassStruct1(ref reader);
        Assert.Equal('d', reader.Read());
    }

    private static void PassStruct1(ref StringSpanReader reader)
    {
        Assert.Equal('b', reader.Read());
        PassStruct2(ref reader);
    }

    private static void PassStruct2(ref StringSpanReader reader) => Assert.Equal('c', reader.Read());

    [Fact]
    public void ReadUntilCharacterWhenFound()
    {
        var reader = new StringSpanReader("1 == 1 && 2 == 2");

        var result = reader.ReadUntilCharacter("&&", StringComparison.OrdinalIgnoreCase);

        Assert.Equal("1 == 1 ", result);
        Assert.Equal('&', reader.Peek());
    }

    [Fact]
    public void ReadUntilCharacterWhenFoundAndInMiddleOfString()
    {
        var reader = new StringSpanReader("1 == 1 && 2 == 2");

        //skip until the second ==
        Assert.Equal("1 ==", reader.Read(4));

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
