using LibraryCore.CommandLineParser;
using LibraryCore.CommandLineParser.Options;
using static LibraryCore.CommandLineParser.Options.CommandConfiguration;

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
        var optionBuilder = new OptionsBuilder();

        static Task<int> RunCommandAsync(InvokeParameters parameters) => Task.FromResult(24);

        optionBuilder.AddCommand(Create("RunReport", "Run this command to generate the report", RunCommandAsync))
                     .AddCommand(Create("ImportFile", "Import file json", RunCommandAsync));

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
        var optionBuilder = new OptionsBuilder();

        static Task<int> RunCommandAsync(InvokeParameters parameters) => Task.FromResult(24);

        optionBuilder.AddCommand(Create("RunReport", "Run this command to generate the report", RunCommandAsync));

        Assert.Equal(1, await Runner.RunAsync(Array.Empty<string>(), optionBuilder));
    }

    [Fact]
    public async Task NoCommandFoundTest()
    {
        var optionBuilder = new OptionsBuilder();

        static Task<int> RunCommandAsync(InvokeParameters parameters) => Task.FromResult(24);

        optionBuilder.AddCommand(Create("RunReport", "Run this command to generate the report", RunCommandAsync));

        const string expectedResult = @"No Command Found For NotFoundCommand
Help Menu

--- Commands ---
? - Help Menu
RunReport - Run this command to generate the report
v - verbose

";

        Assert.Equal(0, await Runner.RunAsync(new[] { "NotFoundCommand" }, optionBuilder));
        Assert.Equal(expectedResult, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public async Task BasicOneCommandTest()
    {
        var optionBuilder = new OptionsBuilder();

        static Task<int> RunCommandAsync(InvokeParameters parameters) => Task.FromResult(24);

        optionBuilder.AddCommand(Create("RunReport", "Run this command to generate the report", RunCommandAsync));

        Assert.Equal(24, await Runner.RunAsync(new[] { "RunReport" }, optionBuilder));
    }

    [Fact]
    public async Task BasicOneCommandWithVerboseTest()
    {
        var optionBuilder = new OptionsBuilder();

        static Task<int> RunCommandAsync(InvokeParameters parameters) => Task.FromResult(24);

        optionBuilder.AddCommand(Create("RunReport", "Run this command to generate the report", RunCommandAsync)
                                .WithRequiredArgument("JsonPath", "Json Path To Use"));

        Assert.Equal(24, await Runner.RunAsync(new[] { "RunReport", "jsonpath123", "-v" }, optionBuilder));

        const string expectedOutput = @"Command To Invoke = RunReport
Required Parameter Name = JsonPath | Value = jsonpath123
";

        Assert.Equal(expectedOutput, Writer.GetStringBuilder().ToString());
    }

    [InlineData("RunReport1", 24)]
    [InlineData("RunReport2", 25)]
    [Theory]
    public async Task BasicTwoCommandTest(string commandToRun, int expectedResult)
    {
        var optionBuilder = new OptionsBuilder();

        static Task<int> RunCommand1(InvokeParameters parameters) => Task.FromResult(24);
        static Task<int> RunCommand2(InvokeParameters parameters) => Task.FromResult(25);

        optionBuilder.AddCommand(Create("RunReport1", "Help Report 1", RunCommand1))
                     .AddCommand(Create("RunReport2", "Help Report 2", RunCommand2));

        Assert.Equal(expectedResult, await Runner.RunAsync(new[] { commandToRun }, optionBuilder));
    }

    [Fact]
    public async Task CommandWithRequiredArgs()
    {
        var optionBuilder = new OptionsBuilder();
        int reportIdToRun = 0;

        Task<int> RunCommand(InvokeParameters parameters)
        {
            reportIdToRun = parameters.RequiredParameterToValue<int>("ReportName");
            return Task.FromResult(1);
        };

        optionBuilder.AddCommand(Create("RunReport", "Run this command to generate the report", RunCommand)
                                 .WithRequiredArgument("ReportName", "Report name to run"));

        Assert.Equal(1, await Runner.RunAsync(new[] { "RunReport", "24" }, optionBuilder));
        Assert.Equal(24, reportIdToRun);
    }

}