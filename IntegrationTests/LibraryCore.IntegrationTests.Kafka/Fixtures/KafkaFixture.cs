
namespace LibraryCore.IntegrationTests.Kafka.Fixtures;

public class KafkaFixture
{

#if DEBUG
    public const string SkipReason = "";// Don't want to run kafka integration test locally for now.";
#else
    public const string SkipReason = "";
#endif

    private static bool RunUnitTest() => string.IsNullOrEmpty(SkipReason);

    public KafkaFixture()
    {
        if (!RunUnitTest())
        {
            return;
        }

        ////so we can debug it in visual studio. Complete hack
        //if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Db_ConnectionString")))
        //{
        //    Console.WriteLine("No DbConnection String Found In Env Variables. Setting Connection String In " + nameof(KafkaFixture));
        //    Environment.SetEnvironmentVariable("Db_ConnectionString", "mongodb://root:Pass!word@localhost:27017");
        //}

        //BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

        //MongoDbClient = new MongoClient(Environment.GetEnvironmentVariable("Db_ConnectionString"));
        //MongoDatabase = MongoDbClient.GetDatabase("IntegrationTest");
    }
}
