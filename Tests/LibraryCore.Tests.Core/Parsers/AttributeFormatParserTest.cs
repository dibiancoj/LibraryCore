﻿//using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

//namespace LibraryCore.Tests.Core.Parsers;

//public class AttributeFormatParserTest : IClassFixture<RuleParserFixture>
//{
//    private RuleParserFixture RuleParserFixture { get; }

//    public AttributeFormatParserTest(RuleParserFixture ruleParserFixture)
//    {
//        RuleParserFixture = ruleParserFixture;
//    }

//    private record MockSaveRequest(int Id, int ProviderId, int? NullableId = null, MockSaveRequest? NestedObject = null);

//    [Fact]
//    public void NoParameters()
//    {
//        Assert.Equal("SaveItemWithNoParameters", RuleParserFixture.ResolveRuleParserEngine()
//                                                            .ParseString("'SaveItemWithNoParameters'")
//                                                            .BuildStringExpression()
//                                                            .Compile()
//                                                            .Invoke());
//    }

//    [Fact]
//    public void SingleParameter()
//    {
//        Assert.Equal("Delete - Prescription Id = 12345", RuleParserFixture.ResolveRuleParserEngine()
//                                                            .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'PrescriptionId')}'")
//                                                            .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                            .Compile()
//                                                            .Invoke(new Dictionary<string, object> { { "PrescriptionId", 12345 } }));
//    }

//    [Fact]
//    public void TwoSimpleParameter()
//    {
//        Assert.Equal("Delete - Prescription Id = 12345 | Doctor Id = 24", RuleParserFixture.ResolveRuleParserEngine()
//                                                                           .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'PrescriptionId')} | Doctor Id = {@ExtractFromDictionary($Request$, 'DoctorId')}'")
//                                                                           .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                                           .Compile()
//                                                                           .Invoke(new Dictionary<string, object> { { "PrescriptionId", 12345 }, { "DoctorId", 24 } }));
//    }

//    [Fact]
//    public void MultipleParameter()
//    {
//        Assert.Equal("Save Request - Drug Name Id = 25 | Provider Id = 9876", RuleParserFixture.ResolveRuleParserEngine()
//                                                                                   .ParseString("'Save Request - Drug Name Id = {@ExtractFromDictionary($Request$, 'SaveRequest.Id')} | Provider Id = {@ExtractFromDictionary($Request$, 'SaveRequest.ProviderId')}'")
//                                                                                   .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                                                   .Compile()
//                                                                                   .Invoke(new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(25, 9876) } }));
//    }

//    [Fact]
//    public void NullablePropertyWithValueOnObject()
//    {
//        Assert.Equal("Delete - Prescription Id = 1", RuleParserFixture.ResolveRuleParserEngine()
//                                                         .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'SaveRequest.NullableId')}'")
//                                                         .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                         .Compile()
//                                                         .Invoke(new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, 1) } }));
//    }

//    [Fact]
//    public void NullablePropertyWithNullValueOnObject()
//    {
//        Assert.Equal("Delete - Prescription Id = ", RuleParserFixture.ResolveRuleParserEngine()
//                                                         .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'SaveRequest.NullableId')}'")
//                                                         .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                         .Compile()
//                                                         .Invoke(new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1) } }));
//    }

//    [Fact]
//    public void TwoLevelNestedObjectWhenPopulated()
//    {
//        Assert.Equal("Delete - Prescription Id = 2", RuleParserFixture.ResolveRuleParserEngine()
//                                                         .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'SaveRequest.NestedObject.Id')}'")
//                                                         .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                         .Compile()
//                                                         .Invoke(new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, NestedObject: new MockSaveRequest(2, 2)) } }));
//    }

//    [Fact]
//    public void TwoLevelNestedObjectWhenNull()
//    {
//        Assert.Equal("Delete - Prescription Id = ", RuleParserFixture.ResolveRuleParserEngine()
//                                                         .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'SaveRequest.NestedObject.Id')}'")
//                                                         .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                         .Compile()
//                                                         .Invoke(new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1) } }));
//    }

//    [Fact]
//    public void TwoLevelNestedObjectWithNullableProperty()
//    {
//        Assert.Equal("Delete - Prescription Id = 22", RuleParserFixture.ResolveRuleParserEngine()
//                                                         .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'SaveRequest.NestedObject.NullableId')}'")
//                                                         .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                         .Compile()
//                                                         .Invoke(new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, NestedObject: new MockSaveRequest(2, 2, 22)) } }));
//    }

//    [Fact]
//    public void TwoLevelNestedObjectWithNullablePropertyThatHasValuie()
//    {
//        Assert.Equal("Delete - Prescription Id = ", RuleParserFixture.ResolveRuleParserEngine()
//                                                         .ParseString("'Delete - Prescription Id = {@ExtractFromDictionary($Request$, 'SaveRequest.NestedObject.NullableId')}'")
//                                                         .BuildStringExpression<Dictionary<string, object>>("Request")
//                                                         .Compile()
//                                                         .Invoke(new Dictionary<string, object> { { "SaveRequest", new MockSaveRequest(1, 1, NestedObject: new MockSaveRequest(2, 2)) } }));
//    }

//    [Fact]
//    public void MultipleParameterBuildExpression()
//    {
//        Assert.Equal("Parameter 1 = Value1 | Parameter 2 = Value2", RuleParserFixture.ResolveRuleParserEngine()
//                                                         .ParseString("'Parameter 1 = {$P1$} | Parameter 2 = {$P2$}'")
//                                                         .BuildStringExpression<string, string>("P1", "P2")
//                                                         .Compile()
//                                                         .Invoke("Value1", "Value2"));
//    }
//}
