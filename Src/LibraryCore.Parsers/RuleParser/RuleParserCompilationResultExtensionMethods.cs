using LibraryCore.Parsers.RuleParser.Logging;
using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser;

public static class RuleParserCompilationResultExtensionMethods
{
    public static Expression<Func<IExpressionLogger, bool>> WithLogging(this Expression<Func<bool>> expression, bool withParameterDebugging = false)
    {
        return (Expression<Func<IExpressionLogger, bool>>)new LoggingExpressionVisitor<Func<IExpressionLogger, bool>>(withParameterDebugging, Expression.Lambda<Func<IExpressionLogger, bool>>).Visit(expression);
    }

    public static Expression<Func<IExpressionLogger, T1, bool>> WithLogging<T1>(this Expression<Func<T1, bool>> expression, bool withParameterDebugging = false)
    {
        return (Expression<Func<IExpressionLogger, T1, bool>>)new LoggingExpressionVisitor<Func<IExpressionLogger, T1, bool>>(withParameterDebugging, Expression.Lambda<Func<IExpressionLogger, T1, bool>>).Visit(expression);
    }

    public static Expression<Func<IExpressionLogger, T1, T2, bool>> WithLogging<T1, T2>(this Expression<Func<T1, T2, bool>> expression, bool withParameterDebugging = false)
    {
        return (Expression<Func<IExpressionLogger, T1, T2, bool>>)new LoggingExpressionVisitor<Func<IExpressionLogger, T1, T2, bool>>(withParameterDebugging, Expression.Lambda<Func<IExpressionLogger, T1, T2, bool>>).Visit(expression);
    }
}
