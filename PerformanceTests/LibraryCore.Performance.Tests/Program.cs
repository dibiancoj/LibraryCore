using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using LibraryCore.CommandLineParser;
using LibraryCore.CommandLineParser.Options;
using LibraryCore.Core.Reflection;
using LibraryCore.Performance.Tests.TestHarnessProvider;

namespace LibraryCore.Performance.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //example to run:
            //dotnet run -c release CsvReader

            var allTestsInAssembly = ReflectionUtility.ScanForAllInstancesOfType<IPerformanceTest>();

            var result = await Runner.RunAsync(args, BuildUpPerformanceTestListCommand(allTestsInAssembly));

            Environment.Exit(result);
        }

        private static OptionsBuilder BuildUpPerformanceTestListCommand(IEnumerable<Type> testsFoundInAssessmbly)
        {
            var options = new OptionsBuilder();

            foreach (var testType in testsFoundInAssessmbly)
            {
                var instanceOfTest = (IPerformanceTest)Activator.CreateInstance(testType);

                options.AddCommand(instanceOfTest.CommandName, instanceOfTest.Description, args =>
                {
                    _ = BenchmarkRunner.Run(testType);

                    return Task.FromResult(0);
                });
            }

            return options;
        }

        public class Config : ManualConfig
        {
            public Config()
            {
                SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithRatioStyle(BenchmarkDotNet.Columns.RatioStyle.Trend); //value is the other basic one
            }
        }
    }
}
