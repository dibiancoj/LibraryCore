namespace LibraryCore.Core.Readers;

/// <summary>
/// This a strict ensure you pass a ref value into any methods otherwise, the Index get's copied resulting in weird issues
/// </summary>
public ref struct StringSpanReader
{
    public StringSpanReader(string stringToParse)
    {
        StringToParse = stringToParse;
        Index = 0;
    }

    private ReadOnlySpan<char> StringToParse { get; }
    private int Index { get; set; }

    public bool HasMoreCharacters() => Index < StringToParse.Length;
    public char? Peek() => HasMoreCharacters() ? StringToParse[Index] : null;
    public char? Read() => HasMoreCharacters() ? StringToParse[Index++] : null;

    public string? Peek(int numberOfCharacters)
    {
        if (!HasMoreCharacters())
        {
            return null;
        }

        return new string(DontHaveEnoughCharactersLeft(numberOfCharacters) ?
                                    StringToParse[Index..] :
                                    StringToParse.Slice(Index, numberOfCharacters));
    }

    public string? Read(int numberOfCharacters)
    {
        if (!HasMoreCharacters())
        {
            return null;
        }

        var stringToReturn = new string(DontHaveEnoughCharactersLeft(numberOfCharacters) ?
                                    StringToParse[Index..] :
                                    StringToParse.Slice(Index, numberOfCharacters));

        //need to fast forward the index. This way its sitting at the character we are up to after the read x amount of characters
        Index += numberOfCharacters;

        return stringToReturn;
    }

    private bool DontHaveEnoughCharactersLeft(int numberOfCharactersToScan) => (Index + numberOfCharactersToScan) > StringToParse.Length;
}
