using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LibraryCore.Core.Parsers.RuleParser.Registration;

public class RuleParserConfiguration
{
    public RuleParserConfiguration(IServiceCollection serviceDescriptors)
    {
        ServiceDescriptors = serviceDescriptors;
        RegisteredMethods = new Dictionary<string, MethodInfo>();
    }

    private IServiceCollection ServiceDescriptors { get; }
    private Dictionary<string, MethodInfo> RegisteredMethods { get; }

    public RuleParserConfiguration WithRegisteredMethod(string key, MethodInfo methodInfo)
    {
        RegisteredMethods.Add(key, methodInfo);
        return this;
    }

    public RuleParserConfiguration WithCustomTokenFactory<T>()
        where T : class, ITokenFactory
    {
        ServiceDescriptors.AddSingleton<ITokenFactory, T>();
        return this;
    }

    public IServiceCollection BuildRuleParser()
    {
        AddRuleParserRules(ServiceDescriptors, RegisteredMethods);
        ServiceDescriptors.AddSingleton<TokenFactoryProvider>();
        ServiceDescriptors.AddSingleton<RuleParserEngine>();

        return ServiceDescriptors;
    }

    private static IServiceCollection AddRuleParserRules(IServiceCollection serviceDescriptors, IDictionary<string, MethodInfo> methodCallRegistrations)
    {
        //data types
        serviceDescriptors.AddSingleton<ITokenFactory, BooleanFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, StringFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, NumberFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, DateFactory>();

        //secondary types
        serviceDescriptors.AddSingleton<ITokenFactory, NullFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, ParameterPropertyFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, WhiteSpaceFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, ArrayFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, LambdaFactory>();
        serviceDescriptors.AddSingleton<ITokenFactory, MethodCallInstanceFactory>();
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
