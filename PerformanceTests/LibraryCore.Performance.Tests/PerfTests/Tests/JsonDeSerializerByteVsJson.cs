using BenchmarkDotNet.Attributes;
using System.Text.Json;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class JsonDeserializerByteVsJson
{
    [GlobalSetup]
    public void Init()
    {
        var myModel = new MyRecord { Id = 99999, Text = "My Really Long Text Value Text Value", SecondaryId = Guid.NewGuid(), IsActive = true, CreatedDate = DateTime.Now, Ids = Enumerable.Range(0, 10).Select(x => x).ToList() };
        ByteArrayData = JsonSerializer.SerializeToUtf8Bytes(myModel);
        StringArrayData = JsonSerializer.Serialize(myModel);
        JsonSerializerOption = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    private JsonSerializerOptions JsonSerializerOption { get; set; }
    private byte[] ByteArrayData { get; set; }
    private string StringArrayData { get; set; }

    [Benchmark(Baseline = true)]
    public MyRecord ByteArray()
    {
        return JsonSerializer.Deserialize<MyRecord>(ByteArrayData, JsonSerializerOption);
    }

    [Benchmark]
    public MyRecord String()
    {
        return JsonSerializer.Deserialize<MyRecord>(StringArrayData, JsonSerializerOption);
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
