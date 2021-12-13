using LibraryCore.Core.Parsers;

namespace LibraryCore.Tests.Core.Parsers;

public class AttributeFormatParserTest
{
    private record MockSaveRequest(int Id, int ProviderId, int? NullableId = null, MockSaveRequest? NestedObject = null);

    [Fact]
    public void NoParameters()
    {
        Assert.Equal("SaveItemWithNoParameters", AttributeFormatParser.ToFormattedString("SaveItemWithNoParameters", null));
    }

    [Fact]
    public void SingleParameter()
    {
        Assert.Equal("Delete - Prescription Id = 12345", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {PrescriptionId}", new Dictionary<string, object> { { "PrescriptionId", 12345 } }));
    }

    [Fact]
    public void SingleParameterButNoDictionaryPassedIn()
    {
        Assert.Throws<Exception>(() =>
        {
            AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {PrescriptionId}", null);
        });
    }

    [Fact]
    public void TwoSimpleParameter()
    {
        Assert.Equal("Delete - Prescription Id = 12345 | Doctor Id = 24",
            AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {PrescriptionId} | Doctor Id = {DoctorId}",
            new Dictionary<string, object> { { "PrescriptionId", 12345 }, { "DoctorId", 24 } }));
    }

    [Fact]
    public void CantFindPropertyPath()
    {
        Assert.Throws<Exception>(() =>
        {
            return AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {PrescriptionId}", new Dictionary<string, object> { { "Id", 12345 } });
        });
    }

    [Fact]
    public void MultipleParameter()
    {
        Assert.Equal("Save Request - Drug Name Id = 25 | Msk Provider Id = 9876",
            AttributeFormatParser.ToFormattedString("Save Request - Drug Name Id = {SaveRequest.Id} | Msk Provider Id = {SaveRequest.ProviderId}",
                                             new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(25, 9876) } }));
    }

    [Fact]
    public void MissingEndBracketThrowsException()
    {
        Assert.Throws<Exception>(() =>
        {
            AttributeFormatParser.ToFormattedString("Missing End Bracket - {Id", new Dictionary<string, object> { { "Id", 12345 } });
        });
    }

    [Fact]
    public void MissingStartBracket()
    {
        Assert.Equal("Missing End Bracket - Id}", AttributeFormatParser.ToFormattedString("Missing End Bracket - Id}", new Dictionary<string, object> { { "Id", 12345 } }));
    }

    [Fact]
    public void NullablePropertyWithValueOnObject()
    {
        Assert.Equal("Delete - Prescription Id = 1", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NullableId}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, 1) } }));
    }

    [Fact]
    public void NullablePropertyWithNullValueOnObject()
    {
        Assert.Equal("Delete - Prescription Id = ", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NullableId}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1) } }));
    }

    [Fact]
    public void TwoLevelNestedObjectWhenPopulated()
    {
        Assert.Equal("Delete - Prescription Id = 2", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NestedObject.Id}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, NestedObject: new MockSaveRequest(2, 2)) } }));
    }

    [Fact]
    public void TwoLevelNestedObjectWhenNull()
    {
        Assert.Equal("Delete - Prescription Id = ", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NestedObject.Id}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1) } }));
    }

    [Fact]
    public void TwoLevelNestedObjectWhenNullWithNullOutputString()
    {
        Assert.Equal("Delete - Prescription Id = null", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NestedObject.Id}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1) } }, "null"));
    }

    [Fact]
    public void TwoLevelNestedObjectWithNullableProperty()
    {
        Assert.Equal("Delete - Prescription Id = 22", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NestedObject.NullableId}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, NestedObject: new MockSaveRequest(2, 2, 22)) } }));
    }

    [Fact]
    public void TwoLevelNestedObjectWithNullablePropertyWithNullOutputString()
    {
        Assert.Equal("Delete - Prescription Id = null", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NestedObject.Id}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1) } }, "null"));
    }

    [Fact]
    public void TwoLevelNestedObjectWithNullablePropertyThatHasValuie()
    {
        Assert.Equal("Delete - Prescription Id = ", AttributeFormatParser.ToFormattedString("Delete - Prescription Id = {SaveRequest.NestedObject.NullableId}", new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, NestedObject: new MockSaveRequest(2, 2)) } }));
    }
}
