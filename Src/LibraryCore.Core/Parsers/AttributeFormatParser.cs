using LibraryCore.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibraryCore.Core.Parsers;

public static class AttributeFormatParser
{

    /// <summary>
    /// Parse a string format when you can't pass objects in. You would enter somethign like: "Add Immunization => Description = {0} | Provider = {1}", "immunizationSaveRequest.Description", "immunizationSaveRequest.MskProvider". Method Parameters would pass in immunizationSaveRequest
    /// </summary>
    /// <param name="format">format string. ie: Add Medication = {request.Id}</param>
    /// <param name="methodParameters">The variables context.ActionArguments</param>
    /// <returns>formatted string that can be outputted straight to audit</returns>
    /// <remarks>internal so we can unit test it</remarks>
    public static string ToFormattedString(string format, IDictionary<string, object> methodParameters)
    {
        //[Audit - "Save Medication = Drug Name Id - {saveRequest.DrugNameId} | Provider - {saveRequest.ProviderId}"

        //this had the best performance. AuditFormatterTest.cs is a comparison using regex which was slower

        if (!format.Contains('{'))
        {
            return format;
        }

        var builder = new StringBuilder();

        using (var reader = new StringReader(format))
        {
            while (reader.HasMoreCharacters())
            {
                var currentCharacterRead = reader.ReadCharacter();

                //append either the replacement value or the character
                builder.Append(currentCharacterRead == '{' ?
                                        MapFormatStatementToValue(FindEndNodeBuffer(reader), methodParameters) :
                                        currentCharacterRead);
            }
        }

        return builder.ToString();
    }

    private static string FindEndNodeBuffer(StringReader reader)
    {
        var buffer = new StringBuilder();

        char currentCharacterRead = default;

        while (reader.HasMoreCharacters() && (currentCharacterRead = reader.ReadCharacter()) != '}')
        {
            buffer.Append(currentCharacterRead);
        }

        //start but no end bracket. throw here
        if (currentCharacterRead != '}')
        {
            throw new Exception("No End Bracket Found In Format");
        }

        return buffer.ToString();
    }

    private static object MapFormatStatementToValue(string nodeStatment, IDictionary<string, object> methodParameters)
    {
        //this would be the "saveRequest.DrugNameId"...split by property value
        var nodePropertiesToParse = nodeStatment.Split('.').AsSpan();

        //will always have atleast 1 record (even if we don't have any items that were split)
        if (!methodParameters.TryGetValue(nodePropertiesToParse[0], out var tempObject))
        {
            throw new Exception("Object Not Found In Parameter Path. Property Path = " + nodePropertiesToParse[0]);
        }

        //loop through all the property levels
        foreach (var propertyLevel in nodePropertiesToParse[1..])
        {
            //grab the property and the value
            tempObject = tempObject!.GetType().GetProperty(propertyLevel)?.GetValue(tempObject, null);
        }

        return tempObject;
    }

}
