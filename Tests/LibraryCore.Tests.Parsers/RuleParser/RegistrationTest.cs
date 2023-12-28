using LibraryCore.Parsers.RuleParser;
using LibraryCore.Parsers.RuleParser.Registration;
using LibraryCore.Parsers.RuleParser.TokenFactories;
using LibraryCore.Parsers.RuleParser.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text.Json;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class RegistrationTest
{
    //the test for the WithConfiguration is in RuleParserFixture
    [Fact]
    public void Basic()
    {
        //validate the AddRuleParser configuration works

        var serviceProvider = new ServiceCollection()
          .AddRuleParser()
          .BuildServiceProvider();

        var ruleParser = serviceProvider.GetRequiredService<RuleParserEngine>();

        Assert.True(ruleParser.ParseString("1 == 1")
                        .BuildExpression()
                        .Compile()
                        .Invoke());
    }

    [Fact]
    public void CustomRuleAlongWithCurrentRules()
    {
        var serviceProvider = new ServiceCollection()
                  .AddRuleParserWithConfiguration()
                        .WithCustomTokenFactory<CustomTokenFactory>()
                  .BuildRuleParser()
               .BuildServiceProvider();

        var ruleParser = serviceProvider.GetRequiredService<RuleParserEngine>();

        var tokens = ruleParser.ParseString("! == 99.99d");

        Assert.IsType<CustomToken>(tokens.CompilationTokenResult[0]);

        Assert.True(tokens
                        .BuildExpression()
                        .Compile()
                        .Invoke());
    }

    public class CustomTokenFactory : ITokenFactory
    {
        public IToken CreateToken(char characterRead, StringReader stringReader, CreateTokenParameters createTokenParameters) => new CustomToken();
        public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == '!';
    }

    public class CustomToken : IToken
    {
        public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters) => Expression.Constant(99.99, typeof(double));
    }
}

