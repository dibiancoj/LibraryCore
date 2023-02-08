using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class ExpressionBuilderTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public ExpressionBuilderTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [Fact]
    public void NoParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2")
                                                .BuildExpression();

        Assert.False(expression.Compile().Invoke());
    }

    [Fact]
    public void NoParameterTestFromCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2")
                                                .BuildExpression()
                                                .Compile();

        Assert.False(expression.Invoke());
    }

    [InlineData(5, false)]
    [InlineData(30, true)]
    [Theory]
    public void SimpleParameterTest(int age, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString($"$Age$ > 15")
                                                .BuildExpression<int>("Age")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(age));
    }

    [Fact]
    public void TwoParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 25 && $Size.SurgeryCount$ == 12 || $Survey.SurgeryCount$ == 24")
                                            .BuildExpression<Survey, Survey>("Survey", "Size");

        //var expression2 = RuleParserFixture.ResolveRuleParserEngine()
        //                                    .ParseString("$Survey.SurgeryCount$ == 25 && $Size.SurgeryCount$ == 12 || $Survey.SurgeryCount$ == 24")
        //                                    .BuildExpression<Survey, Survey, List<string>>("Survey", "Size", "Sb");

        var expression3 = RuleParserFixture.ResolveRuleParserEngine()
                                         .ParseString("$Survey.SurgeryCount$ == 25 && $Size.SurgeryCount$ == 12 || $Survey.SurgeryCount$ == 24")
                                         .BuildExpression<Survey, Survey, ExpressionLogger>("Survey", "Size", "Sb");

        var jasonVisitor = new GetSubExpressionVisitor();

        var newBla = jasonVisitor.Visit(expression3);

        var newBlaCompiled = (Expression<Func<Survey, Survey, ExpressionLogger, bool>>)newBla;
        var t = newBlaCompiled.Compile();
        var sb = new ExpressionLogger();
        var z = t.Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value,
                                                sb);


        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value));
    }

    public class ExpressionLogger
    {
        private List<LogResult> Records { get; } = new();

        public void Add(LogResult logResult) => Records.Add(logResult);

        public IEnumerable<LogResult> LogRecords() => Records.AsEnumerable();
    }

    public record LogResult(string Message, bool Result);

    public class GetSubExpressionVisitor : ExpressionVisitor
    {
        private List<ParameterExpression> Parameters { get; set; } = new();
        private ParameterExpression LoggerParameter { get; set; } = null!;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Parameters.AddRange(node.Parameters);
            LoggerParameter = Parameters.First(x => x.Type == typeof(ExpressionLogger));

            return base.VisitLambda(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Modulo:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return WithLog(node);
            }
            return base.VisitBinary(node);
        }

        public Expression WithLog(BinaryExpression exp)
        {
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

    [Fact]
    public void TwoParameterTestWithCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 24 && $Size.SurgeryCount$ == 12")
                                            .BuildExpression<Survey, Survey>("Survey", "Size")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value));
    }

    [Fact]
    public void ThreeParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
                                            .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(30)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(15)
                                                .Value));
    }

    [Fact]
    public void ThreeParameterTestWithCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
                                            .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(30)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(15)
                                                .Value));
    }

}
