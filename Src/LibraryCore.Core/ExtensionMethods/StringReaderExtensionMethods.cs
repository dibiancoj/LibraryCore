﻿namespace LibraryCore.Core.ExtensionMethods;

public static class StringReaderExtensionMethods
{
    public static bool HasMoreCharacters(this StringReader reader) => reader.Peek() != -1;
    public static char ReadCharacter(this StringReader reader) => (char)reader.Read();
    public static char PeekCharacter(this StringReader reader) => (char)reader.Peek();
}
