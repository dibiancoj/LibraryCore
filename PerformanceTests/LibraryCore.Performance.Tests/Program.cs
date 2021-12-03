using BenchmarkDotNet.Running;
using LibraryCore.Core.Reflection;
using LibraryCore.Performance.Tests.TestHarnessProvider;
using Microsoft.Extensions.CommandLineUtils;

namespace LibraryCore.Performance.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //example to run:
            //dotnet run -c release Union

            var app = new CommandLineApplication()
            {
                Name = "Library Core Performance Tests"
            };

            app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", "1.0.0");

            var allTestsInAssembly = ReflectionUtility.ScanForAllInstancesOfType<IPerformanceTest>();

            //go setup the tests in the command line utils
            BuildUpPerformanceTestListCommand(app, allTestsInAssembly);

            // Executed when no commands are specified
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                app.ShowHelp();
            }
        }

        private static void BuildUpPerformanceTestListCommand(CommandLineApplication app, IEnumerable<Type> testsFoundInAssessmbly)
        {
            foreach (var testType in testsFoundInAssessmbly)
            {
                var instanceOfTest = (IPerformanceTest)Activator.CreateInstance(testType);

                app.Command(instanceOfTest.CommandName, command =>
                {
                    command.Description = instanceOfTest.Description;

                    command.OnExecute(() =>
                    {
                        _ = BenchmarkRunner.Run(testType);

                        return 0;
                    });
                });
            }
        }
    }
}
