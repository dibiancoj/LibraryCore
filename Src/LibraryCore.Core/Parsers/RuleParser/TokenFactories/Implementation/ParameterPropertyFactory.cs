using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class ParameterPropertyFactory : ITokenFactory
{
    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '$';

    public Token CreateToken(char characterRead, StringReader stringReader)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && !char.IsWhiteSpace(stringReader.PeekCharacter()))
        {
            text.Append((char)stringReader.Read());
        }

        var splitText = text.ToString().Split('.');

        return new ParameterPropertyToken(splitText[0], splitText[1]);
    }
}

[DebuggerDisplay("Parameter Property Name = {ParameterName}.{PropertyName}")]
public record ParameterPropertyToken(string ParameterName, string PropertyName) : Token
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters)
    {
        var paremterFound = parameters.SingleOrDefault(x => x.Name.HasValue() && x.Name.Equals(ParameterName, StringComparison.OrdinalIgnoreCase)) ?? throw new Exception($"Parameter Name Not Found: {ParameterName}");

        return Expression.PropertyOrField(paremterFound, PropertyName);
    }
}
