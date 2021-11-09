using System;
using System.Threading;

namespace LibraryCore.Core.DiagnosticUtilities;

public static class DiagnosticUtility
{
    /// <summary>
    /// Run a spin and wait until the timespan passed in has expired
    /// </summary>
    /// <param name="timeSpanToSpinUntil">Time span to spin until. If you pass in 5 seconds then we will spin until that expires</param>
    public static void SpinWaitUntilTimespan(TimeSpan timeSpanToSpinUntil)
    {
        //figure out the time we need to spin until
        var spinUntil = DateTime.Now.Add(timeSpanToSpinUntil);

        //Spin until we are past the spin until variable
        SpinWait.SpinUntil(() => DateTime.Now > spinUntil);
    }
}

