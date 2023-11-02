using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using LibraryCore.Healthcare.Fhir.ExtensionMethods;
using System.Net.Http;

namespace LibraryCore.Tests.Healthcare.Fhir.ExtensionMethods;

public class BundleExtensionMethodTest
{
    private Bundle Bundle { get; set; }
    private Bundle SecondPageBundle { get; }
    private Mock<FhirClient> MockFhirClient { get; }

    public BundleExtensionMethodTest()
    {
        Bundle = new Bundle();
        SecondPageBundle = new Bundle();
        MockFhirClient = new Mock<FhirClient>("http://my.fhirendpoint", new HttpClient(), new FhirClientSettings());

        InitData();
    }

    private void InitData()
    {
        Bundle.Entry.Add(new Bundle.EntryComponent { FullUrl = "abc" });
        Bundle.Entry.Add(new Bundle.EntryComponent { FullUrl = "def" });

        SecondPageBundle.Entry.Add(new Bundle.EntryComponent { FullUrl = "yyy" });
        SecondPageBundle.Entry.Add(new Bundle.EntryComponent { FullUrl = "zzz" });

        MockFhirClient.SetupSequence(x => x.ContinueAsync(Bundle, PageDirection.Next, It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.FromResult<Bundle?>(SecondPageBundle))
            .Returns(System.Threading.Tasks.Task.FromResult<Bundle?>(null));
    }

#if NET7_0_OR_GREATER

    [Fact]
    public void BundleAllPagesInBundleWithBlockingCollection()
    {
        var result = Bundle.AllPagesInBundle(MockFhirClient.Object, default)
                            .ToBlockingEnumerable()
                            .ToArray();

        Assert.Equal(4, result.Count());
        Assert.Equal("abc", result.ElementAt(0).FullUrl);
        Assert.Equal("def", result.ElementAt(1).FullUrl);
        Assert.Equal("yyy", result.ElementAt(2).FullUrl);
        Assert.Equal("zzz", result.ElementAt(3).FullUrl);
    }

#endif

    [Fact]
    public async System.Threading.Tasks.Task BundleAllPagesInBundleWithAsyncForEach()
    {
        var itemsReturned = new List<Bundle.EntryComponent>();

        await foreach(var item in Bundle.AllPagesInBundle(MockFhirClient.Object, default))
        {
            itemsReturned.Add(item);
        }

        Assert.Equal(4, itemsReturned.Count());
        Assert.Equal("abc", itemsReturned.ElementAt(0).FullUrl);
        Assert.Equal("def", itemsReturned.ElementAt(1).FullUrl);
        Assert.Equal("yyy", itemsReturned.ElementAt(2).FullUrl);
        Assert.Equal("zzz", itemsReturned.ElementAt(3).FullUrl);
    }
}
