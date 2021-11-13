using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryCore.Core.Permutations;

/// <summary>
/// Builds the list of possible permutations for a give set of characters for the given length
/// </summary>
public static class PermutationBuilder
{

    //<param name="PermutationItems">Holds the items that make up the different permutation.ie. if you get back "A", "B", "C"...the word would be "ABC". Leaving it like this for numbers to see what you want to do with the numbers individually.</param>
    public record PermutationBuilderResult<T>(IEnumerable<T> PermutationItems)
    {
        /// <summary>
        /// Combines all the permutation items that make up this result. It just does a string builder concat and returns it. One right after the other. If you have numbers and you need something you will need to implement that yourself
        /// </summary>
        /// <returns>All the combined permutation items that make up this result</returns>
        public string PermutationItemsTogether()
        {
            //start the string builder
            var result = new StringBuilder();

            //loop through all the items
            foreach (var itemToAdd in PermutationItems)
            {
                //add the item
                result.Append(itemToAdd);
            }

            //return the result
            return result.ToString();
        }
    }

    #region Public Methods

    #region Total Number Of Permutations

    /// <summary>
    /// Calculates the total number of permutations possible. Overload when you have the list of characters. 
    /// </summary>
    /// <typeparam name="T">Type of items to permutate. Characters or strings</typeparam>
    /// <param name="listToPermute">Characters that will permutate (or numbers if T is a number)</param>
    /// <param name="lengthToPermutate">The length of each row will permutate too.</param>
    /// <param name="itemsAreExclusive">Are items exclusive? Meaning once they are used in the combo, they can't be used in the next items. Example: "abc". Can it be "aaa"? Or once a is used, it can't be used again.</param>
    /// <returns>An array with all the combinations inside</returns>
    /// <returns></returns>
    public static long TotalNumberOfPermutationCombinations<T>(IEnumerable<T> listToPermute, int lengthToPermutate, bool itemsAreExclusive)
    {
        //use the overload (count() does a cast to icollection for optimizations, we don't need to run the same logic)
        return TotalNumberOfPermutationCombinations(listToPermute.Count(), lengthToPermutate, itemsAreExclusive);
    }

    /// <summary>
    /// Calculates the total number of permutations possible. Overload when you know the number of characters you need to permutate
    /// </summary>
    /// <typeparam name="T">Type of items to permutate. Characters or strings</typeparam>
    /// <param name="numberOfCharactersToPermutate">Number of characters to permutate</param>
    /// <param name="lengthToPermutate">The length of each row will permutate too.</param>
    /// <param name="itemsAreExclusive">Are items exclusive? Meaning once they are used in the combo, they can't be used in the next items. Example: "abc". Can it be "aaa"? Or once a is used, it can't be used again.</param>
    /// <returns>An array with all the combinations inside</returns>
    /// <returns></returns>
    public static long TotalNumberOfPermutationCombinations(int numberOfCharactersToPermutate, int lengthToPermutate, bool itemsAreExclusive)
    {
        //Running tally
        long runningTally = 1;

        //running characters to permutate
        int characterCountToPermutate = numberOfCharactersToPermutate;

        //loop through the length we want to permutate
        for (int i = 0; i < lengthToPermutate; i++)
        {
            //multiple by how many characters are left
            runningTally *= characterCountToPermutate;

            //if they are exclusive, remove 1 from the choices of characters we can use
            if (itemsAreExclusive)
            {
                //subtract 1 from the available character count
                characterCountToPermutate--;
            }
        }

        //just return the tally now
        return runningTally;
    }

    #endregion

    #region Permutation Builder

    /// <summary>
    /// Builds the list of possible permutations for a give set of characters for the given length
    /// </summary>
    /// <typeparam name="T">Type of items to permutate. Characters or strings</typeparam>
    /// <param name="listToPermute">Characters that will permutate (or numbers if T is a number)</param>
    /// <param name="lengthToPermutate">The length of each row will permutate too.</param>
    /// <param name="itemsAreExclusive">Are items exclusive? Meaning once they are used in the combo, they can't be used in the next items. Example: "abc". Can it be "aaa"? Or once a is used, it can't be used again.</param>
    /// <returns>An array with all the combinations inside</returns>
    public static IEnumerable<PermutationBuilderResult<T>> BuildPermutationListLazy<T>(IEnumerable<T> listToPermute, int lengthToPermutate, bool itemsAreExclusive)
    {
        //loop through all the permutations
        foreach (var permutations in PermuteLazy(listToPermute, lengthToPermutate, itemsAreExclusive))
        {
            //return this list now
            yield return new PermutationBuilderResult<T>(permutations.PermutationItems);
        }
    }

    #endregion

    #endregion

    #region Private Methods

    /// <summary>
    /// Returns an enumeration of enumerators, one for each permutation of the input.
    /// </summary>
    /// <typeparam name="T">Type of the list</typeparam>
    /// <param name="listToPermute">List to permute </param>
    /// <param name="lengthOfPermute">Length of the word to go to</param>
    /// <param name="itemsAreExclusive">Are items exclusive? Meaning once they are used in the combo, they can't be used in the next items. Example: "abc". Can it be "aaa"? Or once a is used, it can't be used again.</param>
    /// <returns>An array with all the combinations inside</returns>
    private static IEnumerable<PermutationBuilderResult<T>> PermuteLazy<T>(IEnumerable<T> listToPermute, int lengthOfPermute, bool itemsAreExclusive)
    {
        //do we have 0 length to go to?
        if (lengthOfPermute == 0)
        {
            //just return the 0 based index so we can short circuit the rescursive function
            yield return new PermutationBuilderResult<T>(Array.Empty<T>());
        }
        else
        {
            //starting element index
            int startingElementIndex = 0;

            //calculate the length -1 so we don't have to keep calculating it
            int lengthMinus1 = lengthOfPermute - 1;

            //loop through the elements
            foreach (T startingElement in listToPermute)
            {
                //grab the remaining items. Are the items exclusive?
                var remainingItems = itemsAreExclusive ?
                                 listToPermute.Where((x, i) => i != startingElementIndex): //this includes every item but the one at this index
                                 listToPermute;

                //loop through the next set recursively
                foreach (var permutationOfRemainder in PermuteLazy(remainingItems, lengthMinus1, itemsAreExclusive))
                {
                    //go start from the previous call and keep looping (use the iterator so we don't have to allocate a dummy array with 1 element)
                    yield return new PermutationBuilderResult<T>(permutationOfRemainder.PermutationItems.Prepend(startingElement));
                }

                //increase the tally
                startingElementIndex++;
            }
        }
    }

    #endregion

}
