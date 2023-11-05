using Hl7.Fhir.Serialization;
using LibraryCore.Core.Json.Converters;
using LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models;
using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models.BulkFhirCompletedStatus;

namespace LibraryCore.Healthcare.Epic.Fhir.BulkExport;

public class EpicBulkFhirExportApi
{
    public EpicBulkFhirExportApi(HttpClient httpClient, IFhirBearerTokenProvider fhirBearerTokenProvider)
    {
        Client = httpClient;
        FhirBearerTokenProvider = fhirBearerTokenProvider;
        JsonFhirParser = new();
    }

    private HttpClient Client { get; }
    private IFhirBearerTokenProvider FhirBearerTokenProvider { get; }
    private FhirJsonParser JsonFhirParser { get; }
    private static JsonSerializerOptions SerializerOptions { get; } = DefaultCreateSerializerOptions();

    public static JsonSerializerOptions DefaultCreateSerializerOptions()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        serializerOptions.Converters.Add(new BooleanConverter());

        return serializerOptions;
    }

    private async Task<HttpRequestMessage> CreateBaseRequestAsync(HttpMethod method, string url, string acceptType = "application/json")
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await FhirBearerTokenProvider.AccessTokenAsync());
        request.Headers.Add("Accept", acceptType);

        return request;
    }

    public record KickOffBulkRequestResponse(string ContentLocation);

    public async Task<KickOffBulkRequestResponse> KickOffBulkRequestAsync(string url)
    {
        var request = await CreateBaseRequestAsync(HttpMethod.Get, url, "application/fhir+json");

        request.Headers.Add("Prefer", "respond-async");

        var rawResponse = await Client.SendAsync(request);

        return new KickOffBulkRequestResponse(rawResponse.EnsureSuccessStatusCode().Content.Headers.GetValues("Content-Location").First());
    }

    public async Task<IBulkFhirStatus> CheckStatusOfBulkRequestAsync(string contentLocationFromKickOff)
    {
        var request = await CreateBaseRequestAsync(HttpMethod.Get, contentLocationFromKickOff);

        var rawResponse = (await Client.SendAsync(request)).EnsureSuccessStatusCode();

        return rawResponse.StatusCode == HttpStatusCode.Accepted ?
            new BulkFhirInProgressStatus(rawResponse.Headers.GetValues("X-Progress").First()) :
            new BulkFhirCompletedStatus(await rawResponse.Content.ReadFromJsonAsync<BulkFhirCompletedResult>(SerializerOptions) ?? throw new Exception("Can't Deserialize"));
    }

    public async IAsyncEnumerable<T> BulkResultRawSectionData<T>(IEnumerable<string> resultUrls)
        where T : Hl7.Fhir.Model.Base
    {
        foreach (var resultUrl in resultUrls)
        {
            var request = await CreateBaseRequestAsync(HttpMethod.Get, resultUrl);

            var rawResponse = await Client.SendAsync(request);

            using var sr = new StreamReader(await rawResponse.EnsureSuccessStatusCode().Content.ReadAsStreamAsync());
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                yield return await JsonFhirParser.ParseAsync<T>(line);
            }
        }
    }

    public async Task DeleteBulkRequestAsync(string contentLocationFromKickOff)
    {
        var request = await CreateBaseRequestAsync(HttpMethod.Delete, contentLocationFromKickOff);

        _ = (await Client.SendAsync(request)).EnsureSuccessStatusCode();
    }

    /// <summary>
    /// This method is the accumulation of all the methods above. This will kick off a call and poll it until complete. Then once complete will return the data.
    /// Use this if you are only returning 1 type. Otherwise, you can write the few lines of code to combine all the steps
    /// </summary>
    /// <typeparam name="T">Resource type to return</typeparam>
    public async IAsyncEnumerable<T> KickOffAndWaitForCompletionAsync<T>(string kickOffRequestUrl,
                                                                         bool deleteAfterGrabbingData = false,
                                                                         TimeSpan? pollForCompletion = null,
                                                                         [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : Hl7.Fhir.Model.Base
    {
        var kickOffResult = await KickOffBulkRequestAsync(kickOffRequestUrl);
        var bulkFhirCompletedResultOutput = Enumerable.Empty<BulkFhirCompletedResultOutput>();
        var pollTimeFrame = pollForCompletion ?? TimeSpan.FromSeconds(30);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(pollTimeFrame, cancellationToken);

            var statusResult = await CheckStatusOfBulkRequestAsync(kickOffResult.ContentLocation);

            if (statusResult is BulkFhirCompletedStatus completedDataModel)
            {
                bulkFhirCompletedResultOutput = completedDataModel.Result.Output;
                break;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        //var patientsInBulk = bulkFhirCompletedResultOutput.Where(t => t.Type == "Patient"); (need this otherwise we can have mix match)

        await foreach (var resource in BulkResultRawSectionData<T>(bulkFhirCompletedResultOutput.Select(t => t.Url)))
        {
            yield return resource;
        }

        if (deleteAfterGrabbingData)
        {
            await DeleteBulkRequestAsync(kickOffResult.ContentLocation);
        }
    }
}
