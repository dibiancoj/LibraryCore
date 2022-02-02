//using LibraryCore.Core.ExtensionMethods;
//using System.Diagnostics;
//using System.Linq.Expressions;
//using System.Text;

//namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

//public class SurveyAnswerFactory : ITokenFactory
//{
//    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '@';

//    public Token CreateToken(char characterRead, StringReader stringReader)
//    {
//        var text = new StringBuilder();

//        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
//        {
//            text.Append(stringReader.ReadCharacter());
//        }

//        return new SurveyAnswerToken(Convert.ToInt32(text.ToString()));
//    }
//}

//[DebuggerDisplay("Survey Answer {QuestionId}")]
//public record SurveyAnswerToken(int QuestionId) : Token
//{
//    public override Expression CreateExpression(ParameterExpression surveyParameter) => Expression.Call(typeof(MetaCalls).GetMethod(nameof(MetaCalls.GetAnswer)!, new Type[] { typeof(Survey), typeof(int) })!, surveyParameter, Expression.Constant(QuestionId));
//}