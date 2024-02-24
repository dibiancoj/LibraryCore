using Hl7.Fhir.Serialization;
using LibraryCore.ApiClient;
using LibraryCore.Core.Json.Converters;
using LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models;
using LibraryCore.Healthcare.Fhir.MessageHandlers.AuthenticationHandler.TokenBearerProviders;
using LibraryCore.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static LibraryCore.ApiClient.ContentTypeLookup;
using static LibraryCore.Healthcare.Epic.Fhir.BulkExport.Models.BulkFhirCompletedStatus;

namespace LibraryCore.Healthcare.Epic.Fhir.BulkExport;

public class EpicBulkFhirExportApi(HttpClient httpClient, IFhirBearerTokenProvider fhirBearerTokenProvider)
{
    private FhirJsonParser JsonFhirParser { get; } = new();
    private static JsonSerializerOptions SerializerOptions { get; } = DefaultCreateSerializerOptions();

    private static JsonSerializerOptions DefaultCreateSerializerOptions()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        serializerOptions.Converters.Add(new BooleanConverter());

        return serializerOptions;
    }

    private async Task<FluentRequest> CreateBaseRequestAsync(HttpMethod method, string url, AcceptTypeEnum acceptType = AcceptTypeEnum.Json)
    {
        return new FluentRequest(method, url)
                    .AddAuthenticationHeader("Bearer", await fhirBearerTokenProvider.AccessTokenAsync())
                    .AddAcceptType(acceptType);
    }

    public record KickOffBulkRequestResponse(string ContentLocation);

    public async Task<KickOffBulkRequestResponse> KickOffBulkRequestAsync(string url)
    {
        var request = (await CreateBaseRequestAsync(HttpMethod.Get, url, AcceptTypeEnum.FhirJson))
                            .AddHeader("Prefer", "respond-async");

        var rawResponse = await httpClient.SendAsync(request);

        return new KickOffBulkRequestResponse(rawResponse.EnsureSuccessStatusCode().Content.Headers.GetValues("Content-Location").First());
    }

    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
    public async Task<IBulkFhirStatus> CheckStatusOfBulkRequestAsync(string contentLocationFromKickOff)
    {
        var request = await CreateBaseRequestAsync(HttpMethod.Get, contentLocationFromKickOff);

        var rawResponse = (await httpClient.SendAsync(request)).EnsureSuccessStatusCode();

        return rawResponse.StatusCode == HttpStatusCode.Accepted ?
            new BulkFhirInProgressStatus(rawResponse.Headers.GetValues("X-Progress").First()) :
            new BulkFhirCompletedStatus(await rawResponse.Content.ReadFromJsonAsync<BulkFhirCompletedResult>(SerializerOptions) ?? throw new Exception("Can't Deserialize"));
    }

    public async IAsyncEnumerable<T> BulkResultRawSectionData<T>(IEnumerable<string> resultUrls, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : Hl7.Fhir.Model.Base
    {
        foreach (var resultUrl in resultUrls)
        {
            var request = await CreateBaseRequestAsync(HttpMethod.Get, resultUrl);

            var rawResponse = await httpClient.SendAsync(request, cancellationToken);

            using var sr = new StreamReader(await rawResponse.EnsureSuccessStatusCode().Content.ReadAsStreamAsync(cancellationToken));
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return await JsonFhirParser.ParseAsync<T>(line);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
        }
    }

    public async Task DeleteBulkRequestAsync(string contentLocationFromKickOff)
    {
        var request = await CreateBaseRequestAsync(HttpMethod.Delete, contentLocationFromKickOff);

        _ = (await httpClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    /// <summary>
    /// This method is the accumulation of all the methods above. This will kick off a call and poll it until complete. Then once complete will return the data.
    /// Use this if you are only returning 1 type. Otherwise, you can write the few lines of code to combine all the steps
    /// </summary>
    /// <typeparam name="T">Resource type to return</typeparam>
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
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

        await foreach (var resource in BulkResultRawSectionData<T>(bulkFhirCompletedResultOutput.Select(t => t.Url), cancellationToken))
        {
            yield return resource;
        }

        if (deleteAfterGrabbingData)
        {
            await DeleteBulkRequestAsync(kickOffResult.ContentLocation);
        }
    }
}
