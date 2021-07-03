using LibraryCore.Core.DiagnosticUtilities;
using System;
using Xunit;

namespace LibraryCore.Tests.Core.DiagnosticUtilities
{
    public class DiagnosticUtilityTest
    {
        [Fact]
        public void SpinWaitUntilTimespanTest1()
        {
            //grab now
            DateTime now = DateTime.Now;

            //spin until then
            DiagnosticUtility.SpinWaitUntilTimespan(new TimeSpan(0, 0, 2));

            //grab the current time
            var timeNow = DateTime.Now.Subtract(now).Seconds;

            //give a little bit of a buffer to finish..so check 3 to 3.75 seconds
            Assert.True(timeNow >= 2 && timeNow < 2.75);
        }
    }
}
