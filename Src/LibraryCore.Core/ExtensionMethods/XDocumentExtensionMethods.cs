using System.IO;
using System.Text;
using System.Xml.Linq;

namespace LibraryCore.Core.ExtensionMethods;

public static class XDocumentExtensionMethods
{
    /// <summary>
    /// Grab the Document with the declaration. ie: <?xml version='1.0' encoding='utf-8'?>
    /// </summary>
    /// <param name="xDocument">XDocument to render to a string with the declaration</param>
    /// <returns>string representation of the xdocument with the declaration</returns>
    /// <remarks>Not adding an async version since this is all in memory and no IO operation is being used</remarks>
    public static string ToStringWithDeclaration(this XDocument xDocument)
    {
        var builder = new StringBuilder();

        using (var writer = new StringWriter(builder))
        {
            xDocument.Save(writer);
        }

        return builder.ToString();
    }
}
