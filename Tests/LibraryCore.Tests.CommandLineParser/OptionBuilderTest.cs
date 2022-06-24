using LibraryCore.CommandLineParser.Options;
using System.Text;

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

        static int RunCommand() => 24;

        optionBuilder.AddCommand(RunCommand, "RunReport", "Run this command to generate the report")
                    .Create();

        Assert.Equal(0, optionBuilder.Run(new[] { "?" }));

        Writer.Flush();
        Assert.Equal("Help Menu\r\n\r\n", Writer.GetStringBuilder().ToString());
    }

    [Fact]
    public void BasicOneCommandTest()
    {
        var optionBuilder = new OptionBuilder();

        static int RunCommand() => 24;

        optionBuilder.AddCommand(RunCommand, "RunReport", "Run this command to generate the report")
                    .Create();

        Assert.Equal(24, optionBuilder.Run(new[] { "RunReport" }));
    }

}