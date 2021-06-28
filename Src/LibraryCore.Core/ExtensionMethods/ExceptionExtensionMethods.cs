using System;
using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Core.ExtensionMethods
{
    public static class ExceptionExtensionMethods
    {
        public static bool TryParse<TExceptionType>(this Exception exception, [NotNullWhen(true)] out TExceptionType? exceptionFound)
            where TExceptionType : Exception
        {
            exceptionFound = exception.As<TExceptionType>();

            return exceptionFound != null;
        }
    }
}
