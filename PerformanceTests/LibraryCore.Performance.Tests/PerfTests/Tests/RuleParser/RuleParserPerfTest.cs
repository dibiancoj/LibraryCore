﻿using BenchmarkDotNet.Attributes;
using LibraryCore.Parsers.RuleParser;
using LibraryCore.Parsers.RuleParser.Registration;
using LibraryCore.Parsers.RuleParser.TokenFactories;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.RuleParser;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
[ReturnValueValidator(failOnError: true)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
public class RuleParserPerfTest
{
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
    public IReadOnlyList<IToken> Current() => Parser.ParseString(CodeToParse).CompilationTokenResult;
}
