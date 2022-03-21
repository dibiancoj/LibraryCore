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
        public string Mrn { get; set; }
    }

    [Fact]
    public void NullablePropertyWithValueOnObject()
    { 
        var s1 = "'jason 123'";
        var s2 = RuleParserFixture.ResolveRuleParserEngine().ParseString(s1);
        var s3 = s2.BuildStringExpression().Compile().Invoke();

        ///////////////////////////////////////////////

        var a1 = "'Delete - Prescription Id = {$SaveRequest.MedicationId$} | Mrn = {$SaveRequest.Mrn$}'";
        var a2 = RuleParserFixture.ResolveRuleParserEngine().ParseString(a1);
        var a3 = a2.BuildStringExpression<Request>("SaveRequest").Compile().Invoke(new Request { MedicationId = 24, Mrn = "99999999" });



        var result = RuleParserFixture.ResolveRuleParserEngine().ParseString("'Delete - Prescription Id = {$SaveRequest.MedicationId$} | Mrn = {$SaveRequest.Mrn$}'");

        var parameter1 = Expression.Parameter(typeof(Request), "SaveRequest");

        var tt = result.CompilationTokenResult[0].CreateExpression(new List<ParameterExpression> { parameter1 });


        var z = Expression.Lambda<Func<Request, string>>(tt, parameter1);

        var compiled = z.Compile();

        var result2 = compiled.Invoke(new Request { MedicationId = 24, Mrn = "99999999" });

        Assert.True(false);

        Assert.Equal("Delete - Prescription Id = 1", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NullableId}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, 1) } }));
    }
}
