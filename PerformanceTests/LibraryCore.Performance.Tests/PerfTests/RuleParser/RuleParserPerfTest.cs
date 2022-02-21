using BenchmarkDotNet.Attributes;
using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.Registration;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Performance.Tests.TestHarnessProvider;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace LibraryCore.Performance.Tests.PerfTests.RuleParser;

[SimpleJob]
[MemoryDiagnoser]
public class RuleParserPerfTest : IPerformanceTest
{
    public string CommandName => "RuleParser";
    public string Description => "Run the rule parser to tokenize the items. Basic test to verify performance regression";

    [GlobalSetup]
    public void Init()
    {
        var serviceProvider = new ServiceCollection()
          .AddRuleParser()
          .BuildServiceProvider();

        Parser = serviceProvider.GetRequiredService<RuleParserEngine>();
    }

    private RuleParserEngine Parser { get; set; }

    [Params("1 == 1", "'one' == 'one'", "1 == 1 && 2 == 2 && true == true")]
    public string CodeToParse { get; set; }

    [Benchmark(Baseline = true)]
    public IImmutableList<IToken> Current() => Parser.ParseString(CodeToParse).CompilationTokenResult;
}
