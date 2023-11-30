using System.Linq.Expressions;
using System.Text.Json;

namespace LibraryCore.Parsers.RuleParser.Logging;

internal class LoggingExpressionVisitor<TReturnSignature>(bool addParameterDebugging, Func<Expression, IEnumerable<ParameterExpression>, Expression> creator) : ExpressionVisitor
{
    private IEnumerable<ParameterExpression> Parameters { get; set; } = null!;
    private ParameterExpression LoggerParameter { get; set; } = null!;
    private bool AddParameterDebugging { get; } = addParameterDebugging;
    private Func<Expression, IEnumerable<ParameterExpression>, Expression> Creator { get; } = creator;

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        //add the logger parameter to the lambda. This way we have the logger
        LoggerParameter = Expression.Parameter(typeof(IExpressionLogger));
        Parameters = node.Parameters.Prepend(LoggerParameter);

        //having them pass it in so its not completely generic. This is all internal bits that this dll controls
        var newLambdaWithLoggerParameters = AddParameterDebugging ?
                                                BuildParameterLogging(node) :
                                                Creator(node.Body, Parameters);

        return base.VisitLambda((Expression<TReturnSignature>)newLambdaWithLoggerParameters);
    }

    private Expression<TReturnSignature> BuildParameterLogging<T>(Expression<T> lambdaNode)
    {
        //we essentially want to output each parameter passed in and serialize it to json

        var serializeMethod = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.Serialize), new Type[] { typeof(object), typeof(Type), typeof(JsonSerializerOptions) }) ?? throw new Exception("Can't Find Json Serialize MethodInfo");
        var serializerToUse = Expression.Constant(new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
        var loggerAddParametersMethod = typeof(IExpressionLogger).GetMethod(nameof(IExpressionLogger.AddParameter)) ?? throw new Exception("Can't Find Logger Add Parameter MethodInfo");

        //loop through all the parameters in the lambda and call Logging.AddParamters(ParameterName, JsonSerialize(ParameterValue)
        var parametersToLogExecution = lambdaNode.Parameters
            .Select(x => Expression.Call(LoggerParameter, loggerAddParametersMethod, Expression.Constant(x.Name), Expression.Call(serializeMethod, Expression.Convert(x, typeof(object)), Expression.Constant(x.Type), serializerToUse)));

        return Expression.Lambda<TReturnSignature>(Expression.Block(parametersToLogExecution.Append(lambdaNode.Body).ToArray()), Parameters);
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
        //ExpressionLogger.AddLogRecord("1 == 1", true));
        //Then the original expression statement which does the comparison. The block statement will always return the last expression which is the base.VisitBinary(1 == 1)
        return Expression.Block(
            Expression.Call(
                LoggerParameter,
                LoggerParameter.Type.GetMethod(nameof(IExpressionLogger.AddLogRecord)) ?? throw new Exception("Can't Find Add Method On ExpressionLogger"),
                new Expression[]
                {
                    //call method with the 2 parameters (expression description ie: 1 == 1, expression result ie: true)
                    Expression.Call(Expression.Constant(exp), exp.GetType().GetMethod("ToString") ?? throw new Exception("Can't Find ToString Method")),
                    Expression.Convert(exp,typeof(bool))
                }
            ),
            base.VisitBinary(exp)
        );
    }

}