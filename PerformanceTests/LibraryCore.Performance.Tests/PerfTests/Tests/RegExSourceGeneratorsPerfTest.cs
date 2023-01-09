using BenchmarkDotNet.Attributes;
using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.RegularExpressionUtilities;
using System.Text.RegularExpressions;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.Tests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class RegExSourceGeneratorsPerfTest
{
    [Params("https://www.google456.com Test", "local host test https://localhost/Test123. Then there is https://www.google.com. Then some more text. Bla bla https://www.espn.com")]
    public string TextToParse { get; set; }

    [Benchmark(Baseline = true)]
    public string CurrentCompiled()
    {
        return RegularExpressionUtility.ParseRawUrlIntoHyperLink(TextToParse);
    }

    [Benchmark]
    public string WithSourceGenerators()
    {
        return SourceGeneratorVersion.ParseRawUrlIntoHyperLink(TextToParse);
    }
}

public partial class SourceGeneratorVersion
{
    [GeneratedRegex(@"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.Compiled, 3000)] //3 second timeout
    public static partial Regex MyRegex(); //  <-- Declare the partial method, which will be implemented by the source generator

    public static string ParseRawUrlIntoHyperLink(string htmlToParse)
    {
        if (htmlToParse.IsNullOrEmpty())
        {
            return htmlToParse;
        }

        return MyRegex().Replace(htmlToParse, "<a target='_blank' href='$1'>$1</a>");
    }
}
