using MongoDB.Driver;

namespace LibraryCore.IntegrationTests.Mongo.Fixtures;

public class MongoTextFixture
{

#if DEBUG
    public const string SkipReason = "Don't want to run database integration test locally for now.";
#else
    public const string SkipReason = "";
#endif

    public IMongoClient MongoDbClient { get; } = null!;
    public IMongoDatabase MongoDatabase { get; } = null!;

    private static bool RunUnitTest() => string.IsNullOrEmpty(SkipReason);

    public MongoTextFixture()
    {
        if (!RunUnitTest())
        {
            return;
        }

        //so we can debug it in visual studio. Complete hack
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Db_ConnectionString")))
        {
            Console.WriteLine("No DbConnection String Found In Env Variables. Setting Connection String In " + nameof(MongoTextFixture));
            Environment.SetEnvironmentVariable("Db_ConnectionString", "mongodb://root:Pass!word@localhost:27017");
        }

        //BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

        MongoDbClient = new MongoClient(Environment.GetEnvironmentVariable("Db_ConnectionString"));
        MongoDatabase = MongoDbClient.GetDatabase("IntegrationTest");
    }
}
