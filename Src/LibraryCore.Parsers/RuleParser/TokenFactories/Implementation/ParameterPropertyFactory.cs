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

        bool parameterIsDynamicType = parameter.Type == typeof(JsonElement);

        Expression workingExpression = parameter;

        //if we are using a dynamic object and its a single property...then it won't have any nested items...so we will run the dynamic expression now
        if (parameterIsDynamicType && PropertyPath.Count == 1)
        {
            workingExpression = BuildDynamicPropertyValue(workingExpression, PropertyPath[0], workingSchemaElement);
        }

        foreach (var propertyLevel in PropertyPath.Skip(1))
        {
            workingSchemaElement = workingSchemaElement.HasValue ?
                                        workingSchemaElement.Value.GetProperty(propertyLevel) :
                                        null;

            workingExpression = parameterIsDynamicType ?
                          BuildDynamicPropertyValue(BuildDynamicPropertyExpression(workingExpression, propertyLevel, workingSchemaElement), propertyLevel, workingSchemaElement) :
                          Expression.PropertyOrField(workingExpression, propertyLevel);
        }

        return workingExpression;
    }

    private static Expression BuildDynamicPropertyValue(Expression workingExpression, string propertyName, JsonElement? workingSchemaElement)
    {
        ArgumentNullException.ThrowIfNull(workingSchemaElement, nameof(workingSchemaElement));

        if (workingSchemaElement.Value.ValueKind == JsonValueKind.Object)
        {
            //nested object
            return workingExpression;
        }

        if (!Enum.TryParse<SchemaDataType>(workingSchemaElement.Value.GetString(), true, out var schemaValue))
        {
            throw new Exception("Can't Parse Schema Type For Property Level = " + propertyName);
        }

        return Expression.Call(workingExpression, MethodInfoToConvertValue(schemaValue));
    }

    private static Expression BuildDynamicPropertyExpression(Expression workingExpression,
                                                             string propertyLevel,
                                                             JsonElement? workingSchemaElement)
    {
        ArgumentNullException.ThrowIfNull(workingSchemaElement, nameof(workingSchemaElement));

        return Expression.Call(workingExpression, JsonElementGetProperty, Expression.Constant(propertyLevel));
    }

    [ExcludeFromCodeCoverage]
    private string DebuggerDisplay() => string.Join('.', PropertyPath);

}
