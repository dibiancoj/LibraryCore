using LibraryCore.IntegrationTests.Mongo.Fixtures;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using LibraryCore.Mongo;

namespace LibraryCore.IntegrationTests.Mongo
{
    public class MongoPagingTest : IClassFixture<MongoTextFixture>
    {
        public MongoPagingTest(MongoTextFixture mongoTextFixture)
        {
            MongoTextFixture = mongoTextFixture;
            CollegeCollection = MongoTextFixture.MongoDatabase.GetCollection<College>("College");
        }

        private MongoTextFixture MongoTextFixture { get; }
        private IMongoCollection<College> CollegeCollection { get; }

        public class College
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public Guid TestCase { get; set; }
            public string Name { get; set; } = null!;
            public string State { get; set; } = null!;
            public DateTime CreatedDate { get; set; }
        }

        private async Task<BulkWriteResult<College>> InsertDummyDataAsync(Guid testCase, int howMany)
        {
            var upsertData = Enumerable.Range(0, howMany).Select(x => new College
            {
                Id = ObjectId.GenerateNewId(),
                Name = "College " + x.ToString(),
                State = "MA",
                TestCase = testCase,
                CreatedDate = DateTime.Now.AddDays(-x)
            }).ToList();

            return await CollegeCollection.BulkWriteAsync(upsertData.Select(x => new InsertOneModel<College>(x)).ToList());
        }

        [Fact(Skip = MongoTextFixture.SkipReason)]
        public async Task TestPaging()
        {
            var testCaseId = Guid.NewGuid();
            _ = await InsertDummyDataAsync(testCaseId, 10);

            var filter = Builders<College>.Filter.Where(x => x.TestCase == testCaseId && x.State == "MA");
            var sorter = Builders<College>.Sort.Ascending(x => x.CreatedDate);

            var pagedDataOnPage1 = await CollegeCollection.FindAndPageItemsAsync(1, 5, filter, sorter);

            //check first page
            Assert.Equal(2, pagedDataOnPage1.TotalPages);
            Assert.Equal(1, pagedDataOnPage1.CurrentPage);
            Assert.Equal(10, pagedDataOnPage1.TotalRecords);
            Assert.Equal("College 9", pagedDataOnPage1.Data.First().Name);
            Assert.Equal("College 5", pagedDataOnPage1.Data.Last().Name);

            //grab next page
            var pagedDataOnPage2 = await CollegeCollection.FindAndPageItemsAsync(2, 5, filter, sorter);

            //check the 2nd page
            Assert.Equal(2, pagedDataOnPage2.TotalPages);
            Assert.Equal(2, pagedDataOnPage2.CurrentPage);
            Assert.Equal(10, pagedDataOnPage2.TotalRecords);
            Assert.Equal("College 4", pagedDataOnPage2.Data.First().Name);
            Assert.Equal("College 0", pagedDataOnPage2.Data.Last().Name);

        }
    }
}