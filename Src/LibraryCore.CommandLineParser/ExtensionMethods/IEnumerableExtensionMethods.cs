namespace LibraryCore.CommandLineParser.ExtensionMethods;

internal static class IEnumerableExtensionMethods
{
    public static int? IndexOfByPredicate<T>(this IEnumerable<T> values, Func<T, bool> predicate)
    {
        //putting this here so we don't have to take a reference on library core. Which means someone can grab 2 references of the core code.
        int i = 0;

        foreach (var item in values)
        {
            if (predicate(item))
            {
                return i;
            }

            i++;
        }

        return null;
    }
}
