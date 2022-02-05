namespace LibraryCore.Core.ExtensionMethods;

public static class StringReaderExtensionMethods
{
    public static bool HasMoreCharacters(this StringReader reader) => reader.Peek() != -1;
    public static char ReadCharacter(this StringReader reader) => (char)reader.Read();
    public static char PeekCharacter(this StringReader reader) => (char)reader.Peek();
    public static void EatXNumberOfCharacters(this StringReader reader, int numberOfCharactersToEat)
    {
        for (int i = 0; i < numberOfCharactersToEat; i++)
        {
            _ = reader.Read();
        }
    }
}
