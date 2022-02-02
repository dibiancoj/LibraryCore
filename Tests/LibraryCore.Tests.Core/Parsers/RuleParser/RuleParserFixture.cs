﻿using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserFixture
{
    public RuleParserFixture()
    {
        RuleParserEngineToUse = new RuleParserEngine(new ITokenFactory[]
        {
            new TrueFactory(),
            new FalseFactory(),

            new ParameterPropertyFactory(),
            new WhiteSpaceFactory(),
            new StringFactory(),
            new NumberFactory(),
            new MethodCallFactory()
                   .RegisterNewMethodAlias("MyMethod1", new MethodCallFactory.RegisteredMethodParameters(typeof(RuleParserFixture).GetMethod(nameof(GetAnswerId))!, new []{Expression.Constant(20)})),

            new LessThenOrEqualFactory(),
            new LessThenFactory(),
            new GreaterThenOrEqualFactory(),
            new GreaterThenFactory(),
            new EqualsFactory(),

            new OrElseFactory(),
            new AndAlsoFactory()
        });
    }

    public RuleParserEngine RuleParserEngineToUse { get; }

    public static int GetAnswerId(int questionId) => questionId + 1;
}
