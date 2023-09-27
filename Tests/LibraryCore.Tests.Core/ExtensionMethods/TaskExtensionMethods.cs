using System.Runtime.CompilerServices;

namespace LibraryCore.Tests.Core.ExtensionMethods;

/// <summary>
/// Task based extension methods
/// </summary>
public static class TaskExtensionMethods
{

    /// <summary>
    /// Use a continuation based flow to run code after a task returns successfully
    /// </summary>
    /// <typeparam name="TTaskResult">Result type of the task</typeparam>
    /// <typeparam name="TMethodResult">Result of the method after the continuation</typeparam>
    /// <param name="antecedent">Task to await</param>
    /// <param name="continuation">continuation code to run and return the result of</param>
    /// <returns>The end result task</returns>
    public static async Task<TMethodResult> Then<TTaskResult, TMethodResult>(this Task<TTaskResult> antecedent, Func<TTaskResult, TMethodResult> continuation)
    {
        //run the continuation and return the result
        return continuation(await antecedent);
    }

    /// <summary>
    /// Await the continuation func (inner continue)
    /// </summary>
    /// <typeparam name="TTaskResult">Result type of the task</typeparam>
    /// <typeparam name="TMethodResult">Result of the method after the continuation</typeparam>
    /// <param name="antecedent">Task to await</param>
    /// <param name="continuation">continuation code to run and return the result of</param>
    /// <returns>The end result task</returns>
    public static async Task<TMethodResult> Then<TTaskResult, TMethodResult>(this Task<TTaskResult> antecedent, Func<TTaskResult, Task<TMethodResult>> continuation)
    {
        //run the continuation and return the result
        return await continuation(await antecedent);
    }

}
