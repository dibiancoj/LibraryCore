using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using LibraryCore.Healthcare.Fhir.ExtensionMethods;

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

        Assert.Equal(4, result.Length);
        Assert.Equal("abc", result[0].FullUrl);
        Assert.Equal("def", result[1].FullUrl);
        Assert.Equal("yyy", result[2].FullUrl);
        Assert.Equal("zzz", result[3].FullUrl);
    }

#endif

    [Fact]
    public async System.Threading.Tasks.Task BundleAllPagesInBundleWithAsyncForEach()
    {
        var itemsReturned = new List<Bundle.EntryComponent>();

        await foreach (var item in Bundle.AllPagesInBundle(MockFhirClient.Object, default))
        {
            itemsReturned.Add(item);
        }

        Assert.Equal(4, itemsReturned.Count);
        Assert.Equal("abc", itemsReturned[0].FullUrl);
        Assert.Equal("def", itemsReturned[1].FullUrl);
        Assert.Equal("yyy", itemsReturned[2].FullUrl);
        Assert.Equal("zzz", itemsReturned[3].FullUrl);
    }

    [Fact]
    public async System.Threading.Tasks.Task CancelInBetweenItems()
    {
        var itemsReturnedBeforeCancel = new List<Bundle.EntryComponent>();
        var cancelToken = new CancellationTokenSource();

        await foreach (var item in Bundle.AllPagesInBundle(MockFhirClient.Object, cancelToken.Token))
        {
            if (itemsReturnedBeforeCancel.Count == 0)
            {
                cancelToken.Cancel();
            }

            itemsReturnedBeforeCancel.Add(item);
        }

        Assert.Single(itemsReturnedBeforeCancel);
    }

    [Fact]
    public async System.Threading.Tasks.Task CancelAfterFirstPage()
    {
        var itemsReturnedBeforeCancel = new List<Bundle.EntryComponent>();
        var cancelToken = new CancellationTokenSource();

        await foreach (var item in Bundle.AllPagesInBundle(MockFhirClient.Object, cancelToken.Token))
        {
            if (itemsReturnedBeforeCancel.Count == 1)
            {
                cancelToken.Cancel();
            }

            itemsReturnedBeforeCancel.Add(item);
        }

        Assert.Equal(2, itemsReturnedBeforeCancel.Count);
    }
}
