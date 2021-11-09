using LibraryCore.Core.ExtensionMethods;
using System.Xml.Linq;
using Xunit;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class XDocumentExtensionMethodTest
{
    [Fact]
    public void ToStringWithDeclaration()
    {
        var data = XDocument.Parse(
          @"<?xml version=""1.0"" encoding=""utf-16""?>
                    <SavedSurveyData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd =""http://www.w3.org/2001/XMLSchema"">
                       <SurveyAnswers>
                        <FormQuestionAnswer xsi:type=""FormQuestionAnswerStringValue"" QuestionId=""1"" AnswerTypeId=""StringValue"">
                          <Answer>New York</Answer>
                        </FormQuestionAnswer>
                      </SurveyAnswers>
                    </SavedSurveyData>");

        var parsedNode = XDocument.Parse(data.ToStringWithDeclaration());

        Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?>", parsedNode.Declaration.ToString());
    }
}
