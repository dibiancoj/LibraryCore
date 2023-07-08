using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using static LibraryCore.Parsers.RuleParser.Utilities.SchemaModel;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class ParameterPropertyFactory : ITokenFactory
{
    private const char TokenIdentifier = '$';

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == TokenIdentifier;

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              TokenFactoryProvider tokenFactoryProvider,
                              RuleParserEngine ruleParserEngine,
                              SchemaModel schema)
    {
        var text = new StringBuilder();

        while (stringReader.HasMoreCharacters() && stringReader.PeekCharacter() != TokenIdentifier)
        {
            text.Append(stringReader.ReadCharacter());
        }

        //eat the closing $
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, TokenIdentifier);

        return new ParameterPropertyToken(text.ToString().Split('.'), schema);
    }
}

[DebuggerDisplay("Parameter Property Path = {DebuggerDisplay()}")]
public record ParameterPropertyToken(IList<string> PropertyPath, SchemaModel schemaConfiguration) : IToken
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters)
    {
        //need to handle a few scenarios
        //A property off of a single parameter which is an object. ie: $MyParameter.Age$
        //A property which is an int (non-object). ie: $MyInt
        //Multiple parameters. ie: $Parameter1.Age and we have a $Parameter2$

        //loop through each level and keep grabbing the next level

        var parameter = parameters.Single(x => x.Name == PropertyPath[0]);
        JsonElement? workingSchemaElement = schemaConfiguration.Schema.HasValue ?
                                                 schemaConfiguration.Schema.Value.GetProperty(PropertyPath[0]) :
                                                 null;

        Expression workingExpression = parameter;

        foreach (var propertyLevel in PropertyPath.Skip(1))
        {
            //TODO: can clean this up and only calculate if schem is filled out
            workingSchemaElement = workingSchemaElement.HasValue ?
                                        workingSchemaElement.Value.GetProperty(propertyLevel) :
                                        null;

            workingExpression = parameter.Type == typeof(object) || parameter.Type == typeof(JsonElement) ?
                          BuildDynamicPropertyExpression(workingExpression, propertyLevel, workingSchemaElement) :
                          Expression.PropertyOrField(workingExpression, propertyLevel);
        }

        return workingExpression;
    }

    private static Expression BuildDynamicPropertyExpression(Expression workingExpression, string propertyLevel, JsonElement? workingSchemaElement)
    {
        ArgumentNullException.ThrowIfNull(workingSchemaElement, nameof(workingSchemaElement));

        var methodInfo = typeof(JsonElement).GetMethod("GetProperty", new[] { typeof(string) }) ?? throw new Exception("Can't Find Get Property On JsonElement");
        var getPropertyMethodCall = Expression.Call(workingExpression, methodInfo, Expression.Constant(propertyLevel));

        if (workingSchemaElement.Value.ValueKind == JsonValueKind.Object)
        {
            //nested object
            return getPropertyMethodCall;
        }

        if (!Enum.TryParse<SchemaDataType>(workingSchemaElement.Value.GetString(), true, out var schemaValue))
        {
            throw new Exception("Can't Parse Schema Type For Property Level = " + propertyLevel);
        }

        return Expression.Call(getPropertyMethodCall, MethodInfoToConvertValue(schemaValue));

        //todo: cache these


        //var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
        //             propertyLevel,
        //             typeof(ParameterPropertyToken),
        //             new[] { Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null) });

        //return Expression.Dynamic(binder, typeof(object), parameterExpression);
    }

    [ExcludeFromCodeCoverage]
    private string DebuggerDisplay() => string.Join('.', PropertyPath);

}
