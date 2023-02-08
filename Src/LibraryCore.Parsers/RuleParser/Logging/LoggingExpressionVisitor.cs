using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser.Logging;

internal class LoggingExpressionVisitor<TReturnSignature> : ExpressionVisitor
{
    private IEnumerable<ParameterExpression> Parameters { get; set; } = null!;
    private ParameterExpression LoggerParameter { get; set; } = null!;
    private Func<Expression, IEnumerable<ParameterExpression>, Expression> Creator { get; }

    public LoggingExpressionVisitor(Func<Expression, IEnumerable<ParameterExpression>, Expression> creator)
    {
        Creator = creator;
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        //add the logger parameter to the lambda. This way we have the logger
        Parameters = node.Parameters.Prepend(Expression.Parameter(typeof(ExpressionLogger)));
        LoggerParameter = Parameters.Single(x => x.Type == typeof(ExpressionLogger));

        //var newLambdaWithLoggerParameter = Expression.Lambda<Func<ExpressionLogger, bool>>(node.Body, Parameters);

        //having them pass it in so its not completely generic. This is all internal bits that this dll controls
        var newLambdaWithLoggerParameters = Creator(node.Body, Parameters);

        //var newLambdaWithLoggerParameters2 = ExpressionLambdaDynamicGenericMakeMethod()
        //        .MakeGenericMethod(typeof(Func<ExpressionLogger, bool>))
        //        .Invoke(null, new object[] { node.Body, Parameters });

        return base.VisitLambda((Expression<TReturnSignature>)newLambdaWithLoggerParameters);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        return node.NodeType switch
        {
            ExpressionType.Modulo or ExpressionType.Equal or ExpressionType.GreaterThanOrEqual or ExpressionType.LessThanOrEqual or ExpressionType.NotEqual or
            ExpressionType.GreaterThan or ExpressionType.LessThan or ExpressionType.And or ExpressionType.AndAlso or ExpressionType.Or or ExpressionType.OrElse => WithLog(node),
            _ => base.VisitBinary(node),
        };
    }

    public Expression WithLog(BinaryExpression exp)
    {
        //ExpressionLogger.Add(new LogResult("MyParam == 5", true);

        return Expression.Block(
            Expression.Call(
                LoggerParameter,
                LoggerParameter.Type.GetMethod("Add", new Type[] { typeof(LogResult) }) ?? throw new Exception("Can't Find Add Method On ExpressionLogger"),
                new[]
                {
                        Expression.New(typeof(LogResult).GetConstructor(new [] { typeof(string), typeof(bool)}) ?? throw new Exception("Can't Find LogResult Constructor"),
                                Expression.Call(Expression.Constant(exp), exp.GetType().GetMethod("ToString") ?? throw new Exception("Can't Find ToString Method")),
                                Expression.Convert(exp,typeof(bool)))
                }
            ),
            base.VisitBinary(exp)
        );

        //return Expression.Block(
        //    Expression.Call(
        //        LoggerParameter,
        //        LoggerParameter.Type.GetMethod("Add", new Type[] { typeof(string) }) ?? throw new Exception("Can't Find Print Method"),
        //        //typeof(StringBuilder).GetMethod("Append", new Type[] { typeof(string) }) ?? throw new Exception("Can't Find Print Method"),
        //        new[]
        //        {
        //        Expression.Call(
        //            typeof(string).GetMethod("Format", new [] { typeof(string), typeof(object), typeof(object)}) ?? throw new Exception("Can't Find Format Method"),
        //            Expression.Constant("Executing Rule: {0} --> {1}"),
        //            Expression.Call(Expression.Constant(exp), exp.GetType().GetMethod("ToString") ?? throw new Exception("Can't Find ToString Method")),
        //            Expression.Convert(
        //                exp,
        //                typeof(object)
        //            )
        //        )
        //        }
        //    ),
        //    base.VisitBinary(exp)
        //);


        //return Expression.Block(
        //    Expression.Call(
        //        typeof(Debug).GetMethod("Print", new Type[] { typeof(string) }) ?? throw new Exception("Can't Find Print Method"),
        //        new[]
        //        {
        //        Expression.Call(
        //            typeof(string).GetMethod("Format", new [] { typeof(string), typeof(object), typeof(object)}) ?? throw new Exception("Can't Find Format Method"),
        //            Expression.Constant("Executing Rule: {0} --> {1}"),
        //            Expression.Call(Expression.Constant(exp), exp.GetType().GetMethod("ToString") ?? throw new Exception("Can't Find ToString Method")),
        //            Expression.Convert(
        //                exp,
        //                typeof(object)
        //            )
        //        )
        //        }
        //    ),
        //    base.VisitBinary(exp)
        //);
    }

}