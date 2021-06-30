using System.Net.Http.Headers;

namespace LibraryCore.Core.ContentType
{
    public static class ContentTypeLookup
    {
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
        /// Json media type so we don't need to create a new instance each time.
        /// </summary>
        public static MediaTypeWithQualityHeaderValue JsonMediaType { get; } = new MediaTypeWithQualityHeaderValue(JsonContentType);

        /// <summary>
        /// Html media type so we don't need to create a new instance each time.
        /// </summary>
        public static MediaTypeWithQualityHeaderValue HtmlMediaType { get; } = new MediaTypeWithQualityHeaderValue(HtmlContentType);

        /// <summary>
        /// text media type so we don't need to create a new instance each time.
        /// </summary>
        public static MediaTypeWithQualityHeaderValue TextMediaType { get; } = new MediaTypeWithQualityHeaderValue(TextPlainContentType);
    }
}
