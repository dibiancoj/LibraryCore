namespace LibraryCore.Core.Readers;

/// <summary>
/// This is faster by a string reader by 2x. Always pass this into a method by ref to avoid copy cost. Always benchmark this over string reader but is faster based on tests
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

        //fast forward the index (because we are working with just the index on...we need to += to fast forward from the index plus whatever the smaller string we are scanning)
        Index += tryToFindIndex;

        //start from the first character and read until the index
        return new string(stringFromIndexToEnd[..tryToFindIndex]);
    }
}
