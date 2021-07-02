using LibraryCore.Core.Permutations;
using System.Linq;
using Xunit;

namespace LibraryCore.Tests.Core.Permutations
{
    public class PermutationBuilderTest
    {

        #region Permutation List

        /// <summary>
        /// Test Permutation for a given set of characters with an exclusive character once it's used
        /// </summary>
        [Fact]
        public void PermutationTestExclusive1()
        {
            //choices we can use
            var choices = new string[] { "a", "b", "c" };

            //length we are going to use
            const int lengthToTest = 2;

            //is exclusive
            const bool isExclusive = true;

            //go build the result
            var result = PermutationBuilder.BuildPermutationListLazy(choices, lengthToTest, isExclusive).ToArray();

            //make sure there are 6 sets
            Assert.Equal(6, result.Length);

            //make sure there are 2 elements for each item
            foreach (var resultDimension in result)
            {
                //make sure the items in this container are 2
                Assert.Equal(2, resultDimension.PermutationItems.Count());
            }

            //we are going to test this against the total number of choices method to make sure everything is in synch. what is returned here matches the total number of choices in the other method
            Assert.Equal(result.LongCount(), PermutationBuilder.TotalNumberOfPermutationCombinations(choices, lengthToTest, isExclusive));

            //check the overload with the count
            Assert.Equal(result.LongCount(), PermutationBuilder.TotalNumberOfPermutationCombinations(choices.Length, lengthToTest, isExclusive));

            //now look through the values
            Assert.Equal("ab", result[0].PermutationItemsTogether());
            Assert.Equal("ac", result[1].PermutationItemsTogether());
            Assert.Equal("ba", result[2].PermutationItemsTogether());
            Assert.Equal("bc", result[3].PermutationItemsTogether());
            Assert.Equal("ca", result[4].PermutationItemsTogether());
            Assert.Equal("cb", result[5].PermutationItemsTogether());
        }

        /// <summary>
        /// Test Permutation for a given set of characters that is not exclusive
        /// </summary>
        [Fact]
        public void PermutationTestNotExclusive1()
        {
            //choices we can use
            var choices = new string[] { "a", "b", "c" };

            //length we are going to use
            const int lengthToTest = 2;

            //is exclusive
            const bool isExclusive = false;

            //go build the result
            var result = PermutationBuilder.BuildPermutationListLazy(choices, lengthToTest, isExclusive).ToArray();

            //make sure there are 6 sets
            Assert.Equal(9, result.Length);

            //make sure there are 2 elements for each item
            foreach (var resultDimension in result)
            {
                //make sure the items in this container are 2
                Assert.Equal(2, resultDimension.PermutationItems.Count());
            }

            //we are going to test this against the total number of choices method to make sure everything is in synch. what is returned here matches the total number of choices in the other method
            Assert.Equal(result.LongCount(), PermutationBuilder.TotalNumberOfPermutationCombinations(choices, lengthToTest, isExclusive));

            //check the overload with the count
            Assert.Equal(result.LongCount(), PermutationBuilder.TotalNumberOfPermutationCombinations(choices.Length, lengthToTest, isExclusive));

            //now look through the values
            Assert.Equal("aa", result[0].PermutationItemsTogether());
            Assert.Equal("ab", result[1].PermutationItemsTogether());
            Assert.Equal("ac", result[2].PermutationItemsTogether());
            Assert.Equal("ba", result[3].PermutationItemsTogether());
            Assert.Equal("bb", result[4].PermutationItemsTogether());
            Assert.Equal("bc", result[5].PermutationItemsTogether());
            Assert.Equal("ca", result[6].PermutationItemsTogether());
            Assert.Equal("cb", result[7].PermutationItemsTogether());
            Assert.Equal("cc", result[8].PermutationItemsTogether());
        }

        #endregion

        #region Permutation Total Choices

        /// <summary>
        /// Test total number of permutation 
        /// </summary>
        [Fact]
        public void TotalNumberOfPermutationsNotExclusive1()
        {
            //parameters for test 1
            var test1Params = new
            {
                Choices = new string[] { "a", "b", "c" },
                Length = 2,
                IsExclusive = false,
                TestResultShouldBe = 3 * 3
            };

            //parameters for test 2
            var test2Params = new
            {
                Choices = new string[] { "a", "b", "c", "d" },
                Length = 3,
                IsExclusive = false,
                TestResultShouldBe = 4 * 4 * 4
            };

            //go test different scenario. basic formula = (number of characters) * (number of characters) * (number of characters) keep multiplying until you get the length which is the 2nd parameter in the method
            //test 1
            Assert.Equal(test1Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test1Params.Choices, test1Params.Length, test1Params.IsExclusive));

            //test 1 - check overload
            Assert.Equal(test1Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test1Params.Choices.Length, test1Params.Length, test1Params.IsExclusive));

            //test 2
            Assert.Equal(test2Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test2Params.Choices, test2Params.Length, test2Params.IsExclusive));

            //test 2 - check overload
            Assert.Equal(test2Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test2Params.Choices.Length, test2Params.Length, test2Params.IsExclusive));
        }

        /// <summary>
        /// Test total number of permutation 
        /// </summary>
        [Fact]
        public void TotalNumberOfPermutationsExclusive1()
        {
            //parameters for test 1
            var test1Params = new
            {
                Choices = new string[] { "a", "b", "c" },
                Length = 2,
                IsExclusive = true,
                TestResultShouldBe = 3 * 2
            };

            //parameters for test 2
            var test2Params = new
            {
                Choices = new string[] { "a", "b", "c", "d" },
                Length = 3,
                IsExclusive = true,
                TestResultShouldBe = 4 * 3 * 2
            };

            //go test different scenario. basic formula = (number of characters) * (number of characters) * (number of characters) keep multiplying until you get the length which is the 2nd parameter in the method
            //test 1
            Assert.Equal(test1Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test1Params.Choices, test1Params.Length, test1Params.IsExclusive));

            //test 1 - check overload
            Assert.Equal(test1Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test1Params.Choices.Length, test1Params.Length, test1Params.IsExclusive));

            //test 2
            Assert.Equal(test2Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test2Params.Choices, test2Params.Length, test2Params.IsExclusive));

            //test 2 - check overload
            Assert.Equal(test2Params.TestResultShouldBe, PermutationBuilder.TotalNumberOfPermutationCombinations(test2Params.Choices.Length, test2Params.Length, test2Params.IsExclusive));
        }

        #endregion

    }
}
