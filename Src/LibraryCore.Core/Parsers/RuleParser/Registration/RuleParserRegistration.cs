using Microsoft.Extensions.DependencyInjection;

namespace LibraryCore.Core.Parsers.RuleParser.Registration;

public static class RuleParserRegistration
{
    /*
        ServiceProvider = new ServiceCollection()
           .AddRuleParser()
               .WithRegisterMethod("MyMethod1", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerId))!)
               .WithRegisterMethod("GetAnswerArray", typeof(RuleParserFixture).GetMethod(nameof(GetAnswerArray))!)
               .WithRegisterMethod("GetNullableIntArray", typeof(RuleParserFixture).GetMethod(nameof(GetNullableIntArray))!)
               .WithRegisterMethod("GetANumberWithNoParameters", typeof(RuleParserFixture).GetMethod(nameof(GetANumberWithNoParameters))!)
           .BuildRuleParser()

           .BuildServiceProvider();

        var ruleParser = ServiceProvider.GetRequiredService<RuleParserEngine>();
    */

    /// <summary>
    /// Call if you don't need any additional configuration such as registered methods.
    /// </summary>
    public static IServiceCollection AddRuleParser(this IServiceCollection serviceDescriptors) => new RuleParserConfiguration(serviceDescriptors).BuildRuleParser();

    /// <summary>
    /// Call if you need additional configuration such as registered methods
    /// </summary>
    /// <returns>RuleParserConfiguration which you can configure and then create the registration</returns>
    public static RuleParserConfiguration AddRuleParserWithConfiguration(this IServiceCollection serviceDescriptors) => new(serviceDescriptors);


}