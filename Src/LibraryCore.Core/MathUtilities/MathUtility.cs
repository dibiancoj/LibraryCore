using System.Linq;

namespace LibraryCore.Core.MathUtilities
{
    public static class MathUtility
    {

        /// <summary>
        /// This this a prime number
        /// </summary>
        /// <param name="numberToCheck">number to check</param>
        /// <returns>True if a prime number</returns>
        public static bool IsPrimeNumber(int numberToCheck)
        {
            var goToNumber = numberToCheck / 2;

            for (var i = 2; i <= goToNumber; i++)
            {
                if (numberToCheck % i == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Is it a composite number ie: not a prime number
        /// </summary>
        /// <param name="numberToCheck">number to check</param>
        /// <returns>True if a composite number</returns>
        public static bool IsCompositeNumber(int numberToCheck) => !IsPrimeNumber(numberToCheck);

    }
}
