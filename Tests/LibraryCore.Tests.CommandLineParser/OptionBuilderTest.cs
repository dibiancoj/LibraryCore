using LibraryCore.CommandLineParser;
using LibraryCore.CommandLineParser.Options;
using static LibraryCore.CommandLineParser.Options.CommandConfiguration;
using static LibraryCore.CommandLineParser.Runner;

namespace LibraryCore.Tests.CommandLineParser;

public class OptionBuilderTest
{
    public OptionBuilderTest()
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

        Assert.Equal(0, await Runner.RunAsync(new[] { "?" }, optionBuilder));

        const string expectedResult = @"Help Menu

--- Commands ---
? - Help Menu
ImportFile - Import file json
RunReport - Run this command to generate the report
v - verbose

";

        Assert.Equal(expectedResult, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task NoCommandArgsFound()
    {
        var optionBuilder = new OptionsBuilder()
                                    .AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(24))
                                    .BuildCommand();

        Assert.Equal(1, await Runner.RunAsync(Array.Empty<string>(), optionBuilder));
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
v - verbose

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

        Assert.Equal(24, await Runner.RunAsync(new[] { "RunReport", "jsonpath123", "-v" }, optionBuilder));

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

    [Fact]
    public async Task CommandWithRequiredArgsThatIsMissing()
    {
        var optionBuilder = new OptionsBuilder().AddCommand("RunReport", "Run this command to generate the report", x => Task.FromResult(1))
                                                .WithRequiredArgument("ReportName", "Report name to run")
                                                .BuildCommand();

        var resultException = await Assert.ThrowsAsync<Exception>(async () =>
        {
            await RunAsync(new[] { "RunReport" }, optionBuilder);
        });

        Assert.Equal("Missing Required Arguments", resultException.Message);
    }

}