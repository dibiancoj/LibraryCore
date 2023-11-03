using static LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models.BulkFhirCompletedStatus;
using System.Text.Json.Serialization;

namespace LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models;

public interface IBulkFhirStatus { };

public record BulkFhirInProgressStatus(string ProgressDescription) : IBulkFhirStatus;

public record BulkFhirCompletedStatus(BulkFhirCompletedResult Result) : IBulkFhirStatus
{
    public record BulkFhirCompletedResult([property: JsonPropertyName("transactionTime")] DateTime TransactionTime,
                                           [property: JsonPropertyName("request")] string Request,
                                           [property: JsonPropertyName("requiresAccessToken")] string RequiresAccessToken,
                                           [property: JsonPropertyName("output")] BulkFhirCompletedResultOutput[] Output,
                                           [property: JsonPropertyName("error")] object[] Error);
}

public record BulkFhirCompletedResultOutput([property: JsonPropertyName("type")] string Type, [property: JsonPropertyName("url")] string Url);