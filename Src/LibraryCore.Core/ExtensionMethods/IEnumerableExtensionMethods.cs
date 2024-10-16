﻿using LibraryCore.Core.ThrowUtilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace LibraryCore.Core.ExtensionMethods;

/// <summary>
/// Extension Methods For IEnumerable
/// </summary>
public static class IEnumerableExtensionMethods
{

    #region Any With Null Check

    /// <summary>
    /// Checks To See If The Collection Has Any Items (Same Idea As The Any Extension Method Of Off IEnumerable). Only Difference Is, If The Collection Is Null It Returns False Instead Of Throwing An Error
    /// </summary>
    /// <typeparam name="T">Type Of The IEnumerable</typeparam>
    /// <param name="collection">Collection To Check Against</param>
    /// <returns>If The Item Has Any Records</returns>
    public static bool AnyWithNullCheck<T>([NotNullWhen(true)] this IEnumerable<T>? collection) => collection.AnyWithNullCheck(null);

    /// <summary>
    /// Checks To See If The Collection Has Any Items (Same Idea As The Any Extension Method Of Off IEnumerable). Only Difference Is, If The Collection Is Null It Returns False Instead Of Throwing An Error
    /// </summary>
    /// <typeparam name="T">Type Of The IEnumerable</typeparam>
    /// <param name="collection">Collection To Check Against</param>
    /// <param name="predicate">Predicate To Query The Collection With A Where Before We Determine If Any Exist</param>
    /// <returns>If The Item Has Any Records</returns>
    public static bool AnyWithNullCheck<T>([NotNullWhen(true)] this IEnumerable<T>? collection, Func<T, bool>? predicate)
    {
        //if it's null then return false
        if (collection == null)
        {
            return false;
        }

        //use the any method with the overload
        return predicate == null ?
                collection.Any() :
                collection.Any(predicate);
    }

    #endregion

    #region None

    /// <summary>
    /// Does the array have no items. 
    /// </summary>
    /// <returns>If the enumerable has no items</returns>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? collection) => !collection.AnyWithNullCheck();

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? collection, Func<T, bool>? predicate) => !collection.AnyWithNullCheck(predicate);

    #endregion

    #region For Each (On IEnumerable)

    /// <summary>
    /// For Each Extension Method. Runs The Action Method For Each Element In The List
    /// </summary>
    /// <typeparam name="T">Type Of The IEnumerable. Don't Need To Pass In</typeparam>
    /// <param name="collectionToProcess">This IEnumerable To Run The Action On</param>
    /// <param name="methodToRunOnEachElement">Method to run on each element.</param>
    /// <remarks>.Net Framework Has For Each Only On List (Ext. Method). This is for anything of IEnumerable.</remarks>
    public static void ForEach<T>(this IEnumerable<T> collectionToProcess, Action<T> methodToRunOnEachElement)
    {
        foreach (T ElementToProcess in collectionToProcess)
        {
            methodToRunOnEachElement.Invoke(ElementToProcess);
        }
    }

    #endregion

    #region Empty If Null

    /// <summary>
    /// Returns an empty ienumerable if the item is null. This way you don't need to check for a null value. ie: a foreach loop
    /// </summary>
    /// <typeparam name="T">Type of Enumerable</typeparam>
    /// <param name="enumerableToCheck">Enumerable to check and return the value off of</param>
    /// <returns>The original IEnumerable (if not null). Or an empty enumerable with 0 elements if the enumerable passed is null</returns>
    /// <remarks>Please note this is slower then checking for null. Multiple reasons including the foreach still allocated the enumerator 'GetEnumerator'</remarks>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? enumerableToCheck) => enumerableToCheck ?? [];

    #endregion

    #region Select Async 

    /// <summary>
    /// Allow you to run a linq select that awaits something. Syntax is a little cleaner using this method
    /// </summary>
    public static async Task<IEnumerable<TOut>> SelectAsync<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, Task<TOut>> selector)
    {
        var results = new List<TOut>();

        foreach (var item in enumerable)
        {
            results.Add(await selector(item));
        }

        return results;
    }

    #endregion

    #region First Index Of Element

    /// <summary>
    /// Gets The First Index Of The Items That Match From The Predicate
    /// </summary>
    /// <typeparam name="T">Type Of The IEnumerable</typeparam>
    /// <param name="collection">Collection To Check Against</param>
    /// <param name="predicate">Predicate To Search For The Element In The Collection</param>
    /// <returns>First Index. Returns Null If Nothing Is Found</returns>
    public static int? FirstIndexOfElement<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        //holds the tally so we can keep track of what index we are up to
        int indexTally = 0;

        //let's loop through the collection now
        foreach (T item in collection)
        {
            //let's test if the predicate returns true
            if (predicate(item))
            {
                //we found the item so just return the index
                return indexTally;
            }

            //let's increase the tally before we go to the next item
            indexTally++;
        }

        //never found a match, return null
        return null;
    }

    #endregion

    #region With Index

    public record WithIndexResult<T>(int Index, T Value);

    /// <summary>
    /// Iterate off an array with T and the index. Allows you to foreach with the index without additional code.
    /// </summary>
    /// <typeparam name="T">Array Element Type</typeparam>
    /// <param name="set">Array to return the set of</param>
    /// <returns>type of index and element item</returns>
    /// <example>foreach (var (index, value) in arrayToTestWith.WithIndex())</example>
    public static IEnumerable<WithIndexResult<T>> WithIndex<T>(this IEnumerable<T> set) => set.Select((value, index) => new WithIndexResult<T>(index, value));

    #endregion

    #region Median

#if NET7_0_OR_GREATER

    /// <summary>
    /// The median is the middle number in a sorted asc or desc, list of numbers and can be more descriptive then the average
    /// </summary>
    /// <typeparam name="TInput">Input type of the source array</typeparam>
    /// <typeparam name="TResult">The result value which needs to be a floating point data type. ie: double, float, decimal, etc.</typeparam>
    /// <param name="source">The source array to grab the median from</param>
    /// <returns>The median value</returns>
    /// <exception cref="ArgumentNullException">Thrown if the source array is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if there are no elements in the array</exception>
    public static TResult Median<TInput, TResult>(this IEnumerable<TInput> source)
        where TInput : INumber<TInput>
        where TResult : INumber<TResult>, IFloatingPoint<TResult>, IDivisionOperators<TResult, TResult, TResult>
    {
        ThrowUtility.ThrowIfNullOrEmpty(source);

        var sortedList = source
            .OrderBy(x => x)
            .ToList();

        var count = sortedList.Count;

        if (count % 2 == 0)
        {
            TInput halfBefore = sortedList[count / 2 - 1];
            TInput half = sortedList[count / 2];

            TResult sum = TResult.CreateChecked(halfBefore + half);

            return sum / TResult.CreateChecked(2);
        }

        return TResult.CreateChecked(sortedList[count / 2]);
    }

#endif

    #endregion

    #region Mode

    /// <summary>
    /// The mode is the number that is repeated most often in a set of numbers
    /// </summary>
    public static IEnumerable<T> Mode<T>(this IEnumerable<T> source)
    {
        if (source.IsNullOrEmpty())
        {
            throw new ArgumentException("Source Is Null Or Empty", nameof(source));
        }

        var groups = source.GroupBy(x => x);
        var maxCount = groups.Max(g => g.Count());

        return groups
            .Where(g => g.Count() == maxCount)
            .Select(g => g.Key);
    }

    #endregion

    #region Partitioning

    /// <summary>
    /// Split a collection into 2 buckets based on the predicate. ie: is even or odd. n => n %%2 == 0
    /// </summary>
    public static (IEnumerable<T> True, IEnumerable<T> False) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var groupByPredicate = source.GroupBy(predicate);

        return (groupByPredicate.FirstOrDefault(t => t.Key) ?? Enumerable.Empty<T>(),
                groupByPredicate.FirstOrDefault(t => !t.Key) ?? Enumerable.Empty<T>());
    }

    #endregion

}
