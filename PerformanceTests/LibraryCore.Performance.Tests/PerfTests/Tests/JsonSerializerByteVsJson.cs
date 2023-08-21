using BenchmarkDotNet.Attributes;
using System.Text.Json;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
[ReturnValueValidator(failOnError: true)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
public class JsonSerializerByteVsJson
{
    [GlobalSetup]
    public void Init()
    {
        var myModel = new MyRecord { Id = 99999, Text = "My Really Long Text Value Text Value", SecondaryId = Guid.NewGuid(), IsActive = true, CreatedDate = DateTime.Now, Ids = Enumerable.Range(0, 10).Select(x => x).ToList() };
        JsonSerializerOption = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    private MyRecord Model { get; set; }
    private JsonSerializerOptions JsonSerializerOption { get; set; }

    [Benchmark(Baseline = true)]
    public byte[] ByteArray()
    {
        return JsonSerializer.SerializeToUtf8Bytes(Model, JsonSerializerOption);
    }

    [Benchmark]
    public string String()
    {
        return JsonSerializer.Serialize(Model, JsonSerializerOption);
    }

    public class MyRecord
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public Guid SecondaryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public IEnumerable<int> Ids { get; set; }
    }

}
