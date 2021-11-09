using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryCore.Core.ExtensionMethods;

public static class DictionaryExtensionMethods
{

    /// <summary>
    /// Note this is on the concurrent dictionary
    /// </summary>
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> creator)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out TValue? valueToFind))
        {
            return valueToFind;
        }

        TValue createdValue = creator();

        dictionary.Add(key, createdValue);

        return createdValue;
    }

    /// <summary>
    /// Note this is on the concurrent dictionary
    /// </summary>
    public static async Task<TValue> GetOrAddAsync<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<Task<TValue>> creator)
         where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out TValue? valueToFind))
        {
            return valueToFind;
        }

        TValue createdValue = await creator();

        dictionary.Add(key, createdValue);

        return createdValue;
    }

}
