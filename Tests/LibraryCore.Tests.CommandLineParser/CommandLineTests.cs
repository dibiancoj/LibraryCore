using LibraryCore.CommandLineParser.Options;
using LibraryCore.CommandLineParser.RunnerModels;
using static LibraryCore.CommandLineParser.Runner;

namespace LibraryCore.Tests.CommandLineParser;

public class CommandLineTests
{
    public CommandLineTests()
    {
        Console.SetOut(Writer);
    }

    private StringWriter Writer { get; set; } = new();

    [Fact]
    public async Task RunHelp()
    {
        var optionBuilder = new OptionsBuilder()
                     .AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(24))
                     .BuildCommand()

                     .AddCommand("ImportFile", "Import file json", x => Task.FromResult(24))
                     .BuildCommand();

        Assert.Equal(0, await RunAsync(new[] { "?" }, optionBuilder));

        const string expectedResult = @"Help Menu

--- Commands ---
? - Help Menu
ImportFile - Import file json
RunReport - Run this command to generate the report
-v - verbose

";

        Assert.Equal(expectedResult, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task RunHelpForSubCommand()
    {
        var optionBuilder = new OptionsBuilder()
                     .AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(24))
                     .WithOptionalArgument("-c", "Connection String To Set", true)
                     .WithOptionalArgument("-t", "Tag to use", false)
                     .BuildCommand();

        Assert.Equal(0, await RunAsync(new[] { "RunReport", "-c", "?" }, optionBuilder));

        const string expectedResult = @"Help Menu

--- Command ---
RunReport
-c - Connection String To Set
-t - Tag to use
-v - verbose

";

        Assert.Equal(expectedResult, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task NoCommandArgsFound()
    {
        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(24))
                                    .BuildCommand();

        Assert.Equal(1, await RunAsync(Array.Empty<string>(), optionBuilder));
    }

    [Fact]
    public async Task NoCommandFoundTest()
    {
        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(24))
                                    .BuildCommand();

        const string expectedResult = @"No Command Registered For NotFoundCommand
Help Menu

--- Commands ---
? - Help Menu
RunReport - Run this command to generate the report
-v - verbose

";

        Assert.Equal(0, await RunAsync(new[] { "NotFoundCommand" }, optionBuilder));
        Assert.Equal(expectedResult, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task BasicOneCommandTest()
    {
        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(24))
                                    .BuildCommand();

        Assert.Equal(24, await RunAsync(new[] { "RunReport" }, optionBuilder));
    }

    [Fact]
    public async Task BasicOneCommandWithVerboseTest()
    {
        var optionBuilder = new OptionsBuilder()
                                .AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(24))
                                .WithRequiredArgument("JsonPath", "Json Path To Use")
                                .BuildCommand();

        Assert.Equal(24, await RunAsync(new[] { "RunReport", "jsonpath123", "-v" }, optionBuilder));

        const string expectedOutput = @"Command To Invoke = RunReport
Required Parameter Name = JsonPath | Value = jsonpath123
";

        Assert.Equal(expectedOutput, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task BasicWithAsyncInvokerTest()
    {
        static async Task<int> InvokerAsyncMethod(InvokeParameters invokeParameter)
        {
            var temp = await Task.FromResult(99);
            return temp;
        }

        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", async x => await InvokerAsyncMethod(x))
                                    .BuildCommand();

        Assert.Equal(99, await RunAsync(new[] { "RunReport" }, optionBuilder));
    }

    [InlineData("RunReport1", 24)]
    [InlineData("RunReport2", 25)]
    [Theory]
    public async Task BasicTwoCommandTest(string commandToRun, int expectedResult)
    {
        var optionBuilder = new OptionsBuilder().AddCommand("RunReport1", "Help Report 1", x => Task.FromResult(24))
                                                .BuildCommand()

                                                .AddCommand("RunReport2", "Help Report 2", x => Task.FromResult(25))
                                                .BuildCommand();

        Assert.Equal(expectedResult, await RunAsync(new[] { commandToRun }, optionBuilder));
    }

    [Fact]
    public async Task CommandWithRequiredArgs()
    {
        int reportIdToRun = 0;

        Task<int> RunCommand(InvokeParameters parameters)
        {
            reportIdToRun = parameters.RequiredParameterToValue<int>("ReportName");
            return Task.FromResult(1);
        };

        var optionBuilder = new OptionsBuilder().AddCommand("RunReport", "Run this command to generate the report", RunCommand)
                                                .WithRequiredArgument("ReportName", "Report name to run")
                                                .BuildCommand();

        Assert.Equal(1, await RunAsync(new[] { "RunReport", "24" }, optionBuilder));
        Assert.Equal(24, reportIdToRun);
    }

    private record TestParameters(int ReportId, string ReportName, DateTime DateToRun, bool OnlyActiveReports);

    [Fact]
    public async Task RequiredParameterWithMultiplValueTypes()
    {
        TestParameters? FoundParameters = null;

        Task<int> RunCommand(InvokeParameters parameters)
        {
            FoundParameters = new TestParameters(parameters.RequiredParameterToValue<int>("ReportId"),
                                                 parameters.RequiredParameterToValue<string>("ReportName"),
                                                 parameters.RequiredParameterToValue<DateTime>("DateToRun"),
                                                 parameters.RequiredParameterToValue<bool>("OnlyActiveReports"));

            return Task.FromResult(1);
        };

        var optionBuilder = new OptionsBuilder().AddCommand("RunReport", "Run this command to generate the report", RunCommand)
                                                .WithRequiredArgument("ReportId", "Report Id To Run")
                                                .WithRequiredArgument("ReportName", "Report name To Run")
                                                .WithRequiredArgument("DateToRun", "Report Date To Run")
                                                .WithRequiredArgument("OnlyActiveReports", "Show Only Active Report")
                                                .BuildCommand();

        var dateToUse = DateTime.Now;

        Assert.Equal(1, await RunAsync(new[] { "RunReport", "24", "MyReport", dateToUse.ToShortDateString().ToString(), true.ToString() }, optionBuilder));
        Assert.Equal(24, FoundParameters?.ReportId);
        Assert.Equal("MyReport", FoundParameters?.ReportName);
        Assert.Equal(dateToUse.ToShortDateString(), FoundParameters?.DateToRun.ToShortDateString());
    }

    [Fact]
    public async Task CommandWithRequiredArgsThatIsMissing()
    {
        var optionBuilder = new OptionsBuilder().AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(1))
                                                .WithRequiredArgument("ReportName", "Report name to run")
                                                .BuildCommand();

        Assert.Equal(1, await RunAsync(new[] { "RunReport" }, optionBuilder));
        Assert.Equal("Missing Required Arguments" + Environment.NewLine, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task OptionalArgumentWithNoCommandAfterFlag()
    {
        string? connectionStringPassedIn = null;

        Task<int> RunCommand(InvokeParameters parameters)
        {
            connectionStringPassedIn = parameters.OptionalParameterToValue<string>("-c").ValueIfSpecified;
            return Task.FromResult(1);
        };

        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", RunCommand)
                                    .WithOptionalArgument("-c", "Connection String", false)
                                    .BuildCommand();

        Assert.Equal(1, await RunAsync(new[] { "RunReport", "-c" }, optionBuilder));
        Assert.Null(connectionStringPassedIn);
    }

    [InlineData("-C")]
    [InlineData("-c")]
    [Theory]
    public async Task OptionalArgumentWithCommandAfterFlag(string flagValueToPassIn)
    {
        string? connectionStringPassedIn = null;

        Task<int> RunCommand(InvokeParameters parameters)
        {
            connectionStringPassedIn = parameters.OptionalParameterToValue<string>("-C").ValueIfSpecified;
            return Task.FromResult(1);
        };

        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", RunCommand)
                                    .WithOptionalArgument("-c", "Connection String", true)
                                    .BuildCommand();

        Assert.Equal(1, await RunAsync(new[] { "RunReport", flagValueToPassIn, "My Connection String" }, optionBuilder));
        Assert.Equal("My Connection String", connectionStringPassedIn);
    }

    [Fact]
    public async Task OptionalArgumentWithCommandThatIsMissing()
    {
        var optionBuilder = new OptionsBuilder()
                                   .AddCommand("RunReport", "Run this command to generate the report", (x) => Task.FromResult(2))
                                   .WithOptionalArgument("-c", "Connection String", true)
                                   .BuildCommand();

        Assert.Equal(1, await RunAsync(new[] { "RunReport", "-c" }, optionBuilder));
        Assert.Equal("Optional Argument Name = -c | Has Missing Optional Arguments" + Environment.NewLine, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task OptionalArgumentTryToResolveButIsNotPassedIn()
    {
        string? connectionStringPassedIn = null;
        bool parameterIsSpecified = true;

        Task<int> RunCommand(InvokeParameters parameters)
        {
            var (IsSpecified, ValueIfSpecified) = parameters.OptionalParameterToValue<string>("-c");

            connectionStringPassedIn = ValueIfSpecified;
            parameterIsSpecified = IsSpecified;

            return Task.FromResult(1);
        };

        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", RunCommand)
                                    .WithOptionalArgument("-c", "Connection String", false)
                                    .BuildCommand();

        Assert.Equal(1, await RunAsync(new[] { "RunReport" }, optionBuilder));

        Assert.Null(connectionStringPassedIn);
        Assert.False(parameterIsSpecified);
    }

}