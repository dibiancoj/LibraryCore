namespace LibraryCore.Core.Readers;

/// <summary>
/// This a strict ensure you pass a ref value into any methods otherwise, the Index get's copied resulting in weird issues.
/// This is faster by a string reader by 2x.
/// </summary>
/// <remarks>See StringReaderVsSpanReadPerfTest for the performance test</remarks>
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
    public char? PeekCharacter() => HasMoreCharacters() ? StringToParse[Index] : null;
    public char ReadCharacter() => StringToParse[Index++];

    public string? PeekCharacter(int numberOfCharacters) => PeekOrReadMultipleCharacters(numberOfCharacters);

    public string? ReadCharacter(int numberOfCharacters)
    {
        var readCharacters = PeekOrReadMultipleCharacters(numberOfCharacters);

        //did we have any characters read...if so then it was successful and we need to fast forward
        if (readCharacters != null)
        {
            //need to fast forward the index. This way its sitting at the character we are up to after the read x amount of characters
            Index += numberOfCharacters;
        }

        return readCharacters;
    }

    private string? PeekOrReadMultipleCharacters(int numberOfCharactersToScan)
    {
        if (!HasMoreCharacters())
        {
            return null;
        }

        return new string((Index + numberOfCharactersToScan) > StringToParse.Length ?
                                    StringToParse[Index..] :
                                    StringToParse.Slice(Index, numberOfCharactersToScan));
    }

    public string? ReadUntilCharacter(string characterToLookFor, StringComparison stringComparison)
    {
        //we could be in the middle of the reader. So we need to start from the index on
        var stringFromIndexToEnd = StringToParse[Index..];

        var tryToFindIndex = stringFromIndexToEnd.IndexOf(characterToLookFor, stringComparison);

        if (tryToFindIndex == -1)
        {
            return null;
        }

        //start from the first character and read until the index
        var tempResult = stringFromIndexToEnd[..tryToFindIndex];

        //fast forward the index
        Index = tryToFindIndex;

        return new string(tempResult);
    }
}
