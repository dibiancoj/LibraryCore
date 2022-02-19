using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LibraryCore.Core.Parsers.RuleParser.ExtensionMethods;

public static class RuleParserRegistration
{
    /*
     * using LibraryCore.Core.Parsers.RuleParser;
       using LibraryCore.Core.Parsers.RuleParser.ExtensionMethods;
       using Microsoft.Extensions.DependencyInjection;

       var serviceProviderBuilder = new ServiceCollection()
               .AddLogging()
               .AddRuleParser();

       var serviceProvider = serviceProviderBuilder.BuildServiceProvider();

       var parser = serviceProvider.GetRequiredService<RuleParserEngine>();

       var tokens = parser.ParseString("24 == 24");

       var expression = RuleParserExpressionBuilder.BuildExpression(tokens);

       var result = expression.Compile().Invoke();
    */

    public static IServiceCollection AddRuleParser(this IServiceCollection serviceDescriptors, params KeyValuePair<string, MethodInfo>[] methodCallRegistrations)
    {
        serviceDescriptors.AddRuleParserRules(methodCallRegistrations);
        serviceDescriptors.AddSingleton<TokenFactoryProvider>();
        serviceDescriptors.AddSingleton<RuleParserEngine>();

        return serviceDescriptors;
    }

    private static IServiceCollection AddRuleParserRules(this IServiceCollection serviceDescriptors, params KeyValuePair<string, MethodInfo>[] methodCallRegistrations)
    {
        //data types
        serviceDescriptors.AddSingleton<ITokenFactory, BooleanFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, BooleanFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, StringFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, NumberFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, DateFactory>();

        //secondary types
        serviceDescriptors.AddSingleton<ITokenFactory, NullFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, ParameterPropertyFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, WhiteSpaceFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, ArrayFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, MethodCallFactory>(x =>
        {
            var instance = new MethodCallFactory();

            foreach (var methodToRegister in methodCallRegistrations)
            {
                instance.RegisterNewMethodAlias(methodToRegister.Key, methodToRegister.Value);
            }

            return instance;
        });

        //comparison operators
        serviceDescriptors.AddSingleton<ITokenFactory, LessThenOrEqualFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, LessThenFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, GreaterThenOrEqualFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, GreaterThenFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, EqualsFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, NotEqualsFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, ContainsFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, LikeFactory>();

        //binary / combination operators
        serviceDescriptors.AddSingleton<ITokenFactory, OrElseFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, AndAlsoFactory>();

        return serviceDescriptors;
    }
}