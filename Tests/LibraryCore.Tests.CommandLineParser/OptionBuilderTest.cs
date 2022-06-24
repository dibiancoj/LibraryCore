using LibraryCore.CommandLineParser.Options;
using static LibraryCore.CommandLineParser.Options.CommandBuilder;

namespace LibraryCore.Tests.CommandLineParser;

public class OptionBuilderTest
{
    public OptionBuilderTest()
    {
        Console.SetOut(Writer);
    }

    private StringWriter Writer { get; set; } = new();

    [Fact]
    public void RunHelp()
    {
        var optionBuilder = new OptionBuilder();

        static int RunCommand(InvokeParameters parameters) => 24;

        optionBuilder.AddCommand(RunCommand, "RunReport", "Run this command to generate the report")
                     .CreateCommand()
                     .AddCommand(RunCommand, "ImportFile", "Import file json")
                  .CreateCommand();

        Assert.Equal(0, optionBuilder.Run(new[] { "?" }));

        const string expectedResult = @"Help Menu

--- Commands ---
? - Help Menu
RunReport - Run this command to generate the report
ImportFile - Import file json

";

        Assert.Equal(expectedResult, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public void NoCommandArgsFound()
    {
        var optionBuilder = new OptionBuilder();

        static int RunCommand(InvokeParameters parameters) => 24;

        optionBuilder.AddCommand(RunCommand, "RunReport", "Run this command to generate the report")
                     .CreateCommand();

        Assert.Equal(1, optionBuilder.Run(Array.Empty<string>()));
    }

    [Fact]
    public void NoCommandFoundTest()
    {
        var optionBuilder = new OptionBuilder();

        static int RunCommand(InvokeParameters parameters) => 24;

        optionBuilder.AddCommand(RunCommand, "RunReport", "Run this command to generate the report")
                     .CreateCommand();

        const string expectedResult = @"No Command Found For NotFoundCommand
Help Menu

--- Commands ---
? - Help Menu
RunReport - Run this command to generate the report

";

        Assert.Equal(1, optionBuilder.Run(new[] { "NotFoundCommand" }));
        Assert.Equal(expectedResult, Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public void BasicOneCommandTest()
    {
        var optionBuilder = new OptionBuilder();

        static int RunCommand(InvokeParameters parameters) => 24;

        optionBuilder.AddCommand(RunCommand, "RunReport", "Run this command to generate the report")
                     .CreateCommand();

        Assert.Equal(24, optionBuilder.Run(new[] { "RunReport" }));
    }

    [InlineData("RunReport1", 24)]
    [InlineData("RunReport2", 25)]
    [Theory]
    public void BasicTwoCommandTest(string commandToRun, int expectedResult)
    {
        var optionBuilder = new OptionBuilder();

        static int RunCommand1(InvokeParameters parameters) => 24;
        static int RunCommand2(InvokeParameters parameters) => 25;

        optionBuilder.AddCommand(RunCommand1, "RunReport1", "Help Report 1")
                     .CreateCommand()

                     .AddCommand(RunCommand2, "RunReport2", "Help Report 2")
                     .CreateCommand();

        Assert.Equal(expectedResult, optionBuilder.Run(new[] { commandToRun }));
    }

    [Fact]
    public void CommandWithRequiredArgs()
    {
        var optionBuilder = new OptionBuilder();
        int reportIdToRun = 0;

        int RunCommand(InvokeParameters parameters)
        {
            reportIdToRun = parameters.RequiredParameterToValue<int>("ReportName");
            return 1;
        };

        optionBuilder.AddCommand(RunCommand, "RunReport", "Run this command to generate the report")
                     .WithRequiredArgument("ReportName", "Report name to run")
                     .CreateCommand();

        Assert.Equal(1, optionBuilder.Run(new[] { "RunReport", "24" }));
        Assert.Equal(24, reportIdToRun);
    }

}