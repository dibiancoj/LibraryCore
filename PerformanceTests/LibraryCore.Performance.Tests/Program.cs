using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace LibraryCore.Performance.Tests;

internal class Program
{
    private static void Main(string[] args)
    {
        //example to run:
        //Get the menu = dotnet run -c release
        //Run a specific test = dotnet run -c release -filter *JsonDeserializerByteVsJson*

        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }

    public class Config : ManualConfig
    {
        public Config()
        {
            SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithRatioStyle(BenchmarkDotNet.Columns.RatioStyle.Trend); //value is the other basic one
        }
    }
}
