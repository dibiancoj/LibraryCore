using LibraryCore.Parsers.RuleParser.Logging;
using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser;

public static class RuleParserCompilationResultExtensionMethods
{
    public static Expression<Func<ExpressionLogger, bool>> WithLogging(this Expression<Func<bool>> expression)
    {
        return (Expression<Func<ExpressionLogger, bool>>)new LoggingExpressionVisitor<Func<ExpressionLogger, bool>>(Expression.Lambda<Func<ExpressionLogger, bool>>).Visit(expression);
    }

    public static Expression<Func<ExpressionLogger, T1, bool>> WithLogging<T1>(this Expression<Func<T1, bool>> expression)
    {
        return (Expression<Func<ExpressionLogger, T1, bool>>)new LoggingExpressionVisitor<Func<ExpressionLogger, T1, bool>>(Expression.Lambda<Func<ExpressionLogger, T1, bool>>).Visit(expression);
    }

    public static Expression<Func<ExpressionLogger, T1, T2, bool>> WithLogging<T1, T2>(this Expression<Func<T1, T2, bool>> expression)
    {
        return (Expression<Func<ExpressionLogger, T1, T2, bool>>)new LoggingExpressionVisitor<Func<ExpressionLogger, T1, T2, bool>>(Expression.Lambda<Func<ExpressionLogger, T1, T2, bool>>).Visit(expression);
    }
}
