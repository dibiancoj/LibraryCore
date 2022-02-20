using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

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

}

