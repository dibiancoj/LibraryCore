using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System.Runtime.CompilerServices;

namespace LibraryCore.Healthcare.Fhir.ExtensionMethods;

public static class BundleExtensionMethods
{
    /// <summary>
    /// Iterate through a bundle and all the pages in the bundle
    /// </summary>
    public static async IAsyncEnumerable<Bundle.EntryComponent> AllPagesInBundle(this Bundle? bundle,
                                                                                 FhirClient client,
                                                                                 [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (bundle != null)
        {
            foreach (var itemInBundle in bundle.Entry)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return itemInBundle;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            bundle = await client.ContinueAsync(bundle, ct: cancellationToken).ConfigureAwait(false);
        }
    }
}
