using LibraryCore.Core.Parsers;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers;

public class AttributeFormatParserTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public AttributeFormatParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private record MockSaveRequest(int Id, int ProviderId, int? NullableId = null, MockSaveRequest? NestedObject = null);

    public class Request
    {
        public int MedicationId { get; set; }
    }

    [Fact]
    public void NullablePropertyWithValueOnObject()
    {
        var t = "'Delete - Prescription Id = {$SaveRequest.MedicationId}'";

        var result = RuleParserFixture.ResolveRuleParserEngine().ParseString(t);

        var parameter1 = Expression.Parameter(typeof(Request), "SaveRequest");

        result.CompilationTokenResult[0].CreateExpression(new List<ParameterExpression> { parameter1 });

        Assert.Equal("Delete - Prescription Id = 1", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NullableId}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, 1) } }));
    }
}
