namespace LibraryCore.Core.RunAndSuppress;

/// <summary>
/// Please only use this when you really need this! This is generally a bad idea but atleast in this method we log the errors and move on. There are times where we need to disregard errors by 3rd party systems.
/// We want a global method so everyone does it correctly and we don't swallow errors
/// </summary>
public static class RunAndSuppressErrors
{
    public record RunAndSupressErrorResult<TResult>(bool Successful, TResult? ResultObject);

    #region Sync Methods

    /// <summary>
    /// Run a method and continue on if an error has occurred. Method gives you the ability to log the error which you should always do.
    /// </summary>
    /// <typeparam name="TResult">Result type of the method to call</typeparam>
    /// <param name="action">Method to run which will run within a context and carry on if any errors occurrs</param>
    /// <param name="errorLogger">On error run the following command. A write to a log should be performed.</param>
    /// <returns>Tuple which contains the flag if its succesful. ResultObject will contain the result of the method if successfull. Otherwise it will be the default value of TResult</returns>
    public static RunAndSupressErrorResult<TResult> RunAndSuppressAnyErrors<TResult>(Func<TResult> action, Action<Exception> errorLogger)
    {
        return RunAndSuppressAnyErrors(action, default, errorLogger);
    }

    /// <summary>
    /// Run a method and continue on if an error has occurred. Method gives you the ability to log the error which you should always do.
    /// </summary>
    /// <typeparam name="TResult">Result type of the method to call</typeparam>
    /// <param name="action">Method to run which will run within a context and carry on if any errors occurrs</param>
    /// <param name="valueOnFailedCalls">The value to be in ResultObject (return object) when the call fails</param>
    /// <param name="errorLogger">On error run the following command. A write to a log should be performed.</param>
    /// <returns>Tuple which contains the flag if its succesful. ResultObject will contain the result of the method if successfull. Otherwise it will be the the parameter value of 'valueOnFailedCalls' which is passed in</returns>
    public static RunAndSupressErrorResult<TResult> RunAndSuppressAnyErrors<TResult>(Func<TResult> action, TResult? valueOnFailedCalls, Action<Exception> errorLogger)
    {
        try
        {
            //run the action
            return new RunAndSupressErrorResult<TResult>(true, action());
        }
        catch (Exception ex)
        {
            errorLogger(ex);

            //action failed, return that model
            return new RunAndSupressErrorResult<TResult>(false, valueOnFailedCalls);
        }
    }

    /// <summary>
    /// Run and execute a method. Supress the exception and record. Use this for methods that don't return anything. ie: Synch void methods
    /// </summary>
    /// <param name="action">Method to run which will run within a context and carry on if any errors occurrs</param>
    /// <param name="errorLogger">On error run the following command. A write to a log should be performed.</param>
    /// <returns>If the action was successful</returns>
    public static bool RunAndSuppressAnyErrors(Action action, Action<Exception> errorLogger)
    {
        try
        {
            action();

            return true;
        }
        catch (Exception ex)
        {
            errorLogger(ex);

            return false;
        }
    }

    #endregion

    #region Async Task Methods

    /// <summary>
    /// Run and execute a method. Supress the exception and record. Use this for methods that don't return anything. ie: Synch void methods
    /// </summary>
    /// <param name="action">Method to run which will run within a context and carry on if any errors occurrs</param>
    /// <param name="errorLogger">On error run the following command. A write to a log should be performed.</param>
    /// <returns>If the action was successful</returns>
    public static async Task<bool> RunAndSuppressAnyErrorsAsync(Func<Task> action, Action<Exception> errorLogger)
    {
        try
        {
            await action();

            return true;
        }
        catch (Exception ex)
        {
            errorLogger(ex);

            return false;
        }
    }

    /// <summary>
    /// Run a method and continue on if an error has occurred. Method gives you the ability to log the error which you should always do.
    /// </summary>
    /// <typeparam name="TResult">Result type of the method to call</typeparam>
    /// <param name="actionAsync">Method to run which will run within a context and carry on if any errors occurrs</param>
    /// <param name="errorLogger">On error run the following command. A write to a log should be performed.</param>
    /// <returns>Tuple which contains the flag if its succesful. ResultObject will contain the result of the method if successfull. Otherwise it will be the default value of TResult</returns>
    public static Task<RunAndSupressErrorResult<TResult>> RunAndSuppressAnyErrorsAsync<TResult>(Func<Task<TResult>> actionAsync, Action<Exception> errorLogger)
    {
        return RunAndSuppressAnyErrorsAsync(actionAsync, () => default, errorLogger);
    }

    /// <summary>
    /// Run a method and continue on if an error has occurred. Method gives you the ability to log the error which you should always do.
    /// </summary>
    /// <typeparam name="TResult">Result type of the method to call</typeparam>
    /// <param name="actionAsync">Method to run which will run within a context and carry on if any errors occurrs</param>
    /// <param name="valueOnFailedCalls">The value to be in ResultObject (return object) when the call fails</param>
    /// <param name="errorLogger">On error run the following command. A write to a log should be performed.</param>
    /// <returns>Tuple which contains the flag if its succesful. ResultObject will contain the result of the method if successfull. Otherwise it will be the the parameter value of 'valueOnFailedCalls' which is passed in</returns>
    public static async Task<RunAndSupressErrorResult<TResult>> RunAndSuppressAnyErrorsAsync<TResult>(Func<Task<TResult>> actionAsync, Func<TResult?> valueOnFailedCalls, Action<Exception> errorLogger)
    {
        try
        {
            //run the action
            return new RunAndSupressErrorResult<TResult>(true, await actionAsync().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            //call the action method passed in
            errorLogger(ex);

            //failed, return the fail tuple value
            return new RunAndSupressErrorResult<TResult>(false, valueOnFailedCalls());
        }
    }

    #endregion

}
