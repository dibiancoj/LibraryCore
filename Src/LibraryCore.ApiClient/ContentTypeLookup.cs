using System.Net.Http.Headers;

namespace LibraryCore.ApiClient;

public static class ContentTypeLookup
{
    public enum AcceptTypeEnum
    {
        Json,
        Xml,
        TextHtml,
        TextPlain
    }

    public static MediaTypeWithQualityHeaderValue RetrieveAcceptType(AcceptTypeEnum acceptType)
    {
        return acceptType switch
        {
            AcceptTypeEnum.Json => JsonMediaType,
            AcceptTypeEnum.TextPlain => TextPlainMediaType,
            AcceptTypeEnum.Xml => XmlMediaType,
            AcceptTypeEnum.TextHtml => TextHtmlMediaType,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Json content type
    /// </summary>
    public const string JsonContentType = "application/json";

    /// <summary>
    /// Html content type
    /// </summary>
    public const string HtmlContentType = "text/html";

    /// <summary>
    /// text / plain
    /// </summary>
    public const string TextPlainContentType = "text/plain";

    /// <summary>
    /// Pdf file download content type
    /// </summary>
    public const string PdfContentType = "application/pdf";

    /// <summary>
    /// Excel download content type
    /// </summary>
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Binary File For Download
    /// </summary>
    public const string BinaryContentType = "application/octet-stream";

    /// <summary>
    /// Pdf file download content type
    /// </summary>
    public const string ZipContentType = "application/zip";

    /// <summary>
    /// Xml content type
    /// </summary>
    public const string XmlContentType = "application/xml";

    /// <summary>
    /// Json media type
    /// </summary>
    public static MediaTypeWithQualityHeaderValue JsonMediaType { get; } = new MediaTypeWithQualityHeaderValue(JsonContentType);

    /// <summary>
    /// Html media type
    /// </summary>
    public static MediaTypeWithQualityHeaderValue TextHtmlMediaType { get; } = new MediaTypeWithQualityHeaderValue(HtmlContentType);

    /// <summary>
    /// text media type
    /// </summary>
    public static MediaTypeWithQualityHeaderValue TextPlainMediaType { get; } = new MediaTypeWithQualityHeaderValue(TextPlainContentType);

    /// <summary>
    /// Xml media type
    /// </summary>
    public static MediaTypeWithQualityHeaderValue XmlMediaType { get; } = new MediaTypeWithQualityHeaderValue(XmlContentType);
}
