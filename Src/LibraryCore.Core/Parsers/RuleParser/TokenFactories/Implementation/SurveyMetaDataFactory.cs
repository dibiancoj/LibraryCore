//using System.Diagnostics;
//using System.Linq.Expressions;
//using System.Text;

//namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

//public class SurveyMetaDataFactory : ITokenFactory
//{
//    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '$';

//    public Token CreateToken(char characterRead, StringReader stringReader)
//    {
//        var text = new StringBuilder();

//        while (stringReader.Peek() > -1 && !char.IsWhiteSpace((char)stringReader.Peek()))
//        {
//            text.Append((char)stringReader.Read());
//        }

//        return new MetaDataToken(text.ToString());
//    }

//}

//[DebuggerDisplay("Survey Meta Data = {PropertyName}")]
//public record MetaDataToken(string PropertyName) : Token
//{
//    public override Expression CreateExpression(ParameterExpression surveyParameter) => Expression.PropertyOrField(surveyParameter, PropertyName);
//}
