using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Core.ExtensionMethods;

public static class ExceptionExtensionMethods
{
    public static bool TryParse<TExceptionType>(this Exception exception, [NotNullWhen(true)] out TExceptionType? exceptionFound)
        where TExceptionType : Exception
    {
        exceptionFound = exception.As<TExceptionType>();

        return exceptionFound != null;
    }

    public static IEnumerable<Exception> ExceptionTree(this Exception exception)
    {
        //let's add the first exception
        yield return exception;

        //if we don't have an inner exception then we have nothing to traverse down the tree. so we will return right away
        if (exception.InnerException == null)
        {
            //just exit the method
            yield break;
        }

        //throw the exception into a variable
        Exception innerExceptionHolder = exception;

        //let's keep looping until we find it or the inner exception is null
        while (innerExceptionHolder.InnerException != null)
        {
            //let's set the variable to the inner exception now
            innerExceptionHolder = innerExceptionHolder.InnerException;

            //let's add this exception to the list
            yield return innerExceptionHolder;
        }
    }
}
