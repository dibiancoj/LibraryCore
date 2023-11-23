using LibraryCore.Core.ExtensionMethods;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class IListExtensionMethodTest
{
    [Fact]
    public void ShiftLeftWithArray()
    {
        int[] collection = [0, 0, 1, 1, 1, 2, 2, 3, 3, 4];
        var distinctValuesToEvalAtBottom = collection.Distinct().Count();
        var lastIndex = collection.Length - 1;

        int i = 1;
        var previousNumber = collection[0];
        var distinctValues = 1;//because we already have the first item

        while (true)
        {
            var currentValue = collection[i];

            if (currentValue < collection[0])
            {
                break;
            }

            if (currentValue == previousNumber)
            {
                collection.ShiftLeft(i);

                //put a marker when to stop. There is better ways of doing this where you don't need this.
                collection[lastIndex] = collection[0] - 1;
            }
            else
            {
                distinctValues++;
                i++;
                previousNumber = collection[i];
            }
        }

        Assert.Equal(distinctValuesToEvalAtBottom, distinctValues);
        Assert.Equal(0, collection[0]);
        Assert.Equal(1, collection[1]);
        Assert.Equal(2, collection[2]);
        Assert.Equal(3, collection[3]);
        Assert.Equal(4, collection[4]);
        //don't care about the rest
    }

    [Fact]
    public void ShiftRightWithArray()
    {
        //kind of a merge join example for the shift right stuff
        int[] nums1 = [1, 2, 3, 0, 0, 0];
        int[] nums2 = [2, 5, 6];

        int nums1_m = 3;
        int nums2_m = 3;

        int end = nums1_m + nums2_m;
        int tally = nums1_m;

        for (int x = 0; x < nums2.Length; x++)
        {
            var nums2Digit = nums2[x];

            for (int i = 0; i < nums1.Length; i++)
            {
                var nums1Digit = nums1[i];

                if (nums1Digit > nums2Digit || (i >= tally))
                {
                    nums1.ShiftRight(i);
                    tally++;
                    nums1[i] = nums2Digit;
                    break;
                }
            }
        }

        Assert.Equal(1, nums1[0]);
        Assert.Equal(2, nums1[1]);
        Assert.Equal(2, nums1[2]);
        Assert.Equal(3, nums1[3]);
        Assert.Equal(5, nums1[4]);
        Assert.Equal(6, nums1[5]);
        Assert.Equal(6, nums1.Length);
    }

}
