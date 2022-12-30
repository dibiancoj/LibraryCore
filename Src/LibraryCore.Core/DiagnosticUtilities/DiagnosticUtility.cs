namespace LibraryCore.Core.DiagnosticUtilities;

public static class DiagnosticUtility
{
    /// <summary>
    /// Run a spin and wait until the timespan passed in has expired. This is different then SpinUntil because you can't run async methods in that one. This is all async
    /// </summary>
    /// <param name="timeSpanToSpinUntil">Time span to spin until. If you pass in 5 seconds then we will spin until that expires</param>
    public static async Task<bool> SpinUntilAsync(Func<ValueTask<bool>> predicate, TimeSpan? pauseInBetween = null, TimeSpan? timeout = null)
    {
        var timeToCutOut = DateTime.Now.Add(timeout ?? TimeSpan.FromMinutes(5));

        while (!await predicate())
        {
            if (DateTime.Now > timeToCutOut)
            {
                return false;
            }

            if (pauseInBetween.HasValue)
            {
                await Task.Delay(pauseInBetween.Value);
            }
        }

        return true;
    }
}

