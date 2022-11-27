using LibraryCore.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LibraryCore.Tests.Mongo;

public class MongoTest
{
    public class Countries
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; } = null!;
        public IEnumerable<string> States { get; set; } = Enumerable.Empty<string>();
        public DateTime CreatedDate { get; set; } = new DateTime(2020, 1, 1);

        public static IEnumerable<Countries> MockData(int howMany) => Enumerable.Range(1, howMany).Select(x => new Countries
        {
            Id = ObjectId.GenerateNewId(),
            Name = x.ToString(),
            CreatedDate = DateTime.Now,
            States = Enumerable.Range(1, howMany).Select(t => t.ToString())
        });
    }

    public MongoTest()
    {
        MockIMongoCollection = new Mock<IMongoCollection<Countries>>();
    }

    private Mock<IMongoCollection<Countries>> MockIMongoCollection { get; }

    //[Fact]
    //public async Task Upsert_WillInsertRecord()
    //{
    //    var mockNewIdOnInsert = ObjectId.GenerateNewId();

    //    MockIMongoCollection.Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Countries>>(),
    //                                                     It.IsAny<UpdateDefinition<Countries>>(),
    //                                                     It.Is<UpdateOptions>(t => t.IsUpsert == true),
    //                                                     It.IsAny<CancellationToken>()))
    //        .Returns(Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(0, 1, mockNewIdOnInsert)));

    //    var result = await MockIMongoCollection.Object.UpsertOneAsync(
    //                        mockNewIdOnInsert,
    //                        Builders<Countries>.Filter.Where(predicate => predicate.Id == mockNewIdOnInsert),
    //                        Builders<Countries>.Update
    //                                .Set(x => x.CreatedDate, DateTime.Now));

    //    Assert.Equal(UpsertResponse.UpsertModelEnum.Inserted, result.OperationExecuted);
    //    Assert.Equal(mockNewIdOnInsert, result.DocumentId);

    //    MockIMongoCollection.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Countries>>(),
    //                                                      It.IsAny<UpdateDefinition<Countries>>(),
    //                                                      It.Is<UpdateOptions>(t => t.IsUpsert == true),
    //                                                      It.IsAny<CancellationToken>()), Times.Once);
    //}

    //[Fact]
    //public async Task Upsert_WillUpdateRecord()
    //{
    //    var mockNewIdOnInsert = ObjectId.GenerateNewId();
    //    var updatedDate = DateTime.Now;

    //    MockIMongoCollection.Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Countries>>(),
    //                                                     It.IsAny<UpdateDefinition<Countries>>(),
    //                                                     It.Is<UpdateOptions>(t => t.IsUpsert == true),
    //                                                     It.IsAny<CancellationToken>()))
    //        .Returns(Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(1, 0, mockNewIdOnInsert)));

    //    var result = await MockIMongoCollection.Object.UpsertOneAsync(
    //                        mockNewIdOnInsert,
    //                        Builders<Countries>.Filter.Where(predicate => predicate.Id == mockNewIdOnInsert),
    //                        Builders<Countries>.Update
    //                                .Set(x => x.CreatedDate, updatedDate));

    //    Assert.Equal(UpsertResponse.UpsertModelEnum.Updated, result.OperationExecuted);
    //    Assert.Equal(mockNewIdOnInsert, result.DocumentId);

    //    MockIMongoCollection.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Countries>>(),
    //                                                      It.IsAny<UpdateDefinition<Countries>>(),
    //                                                      It.Is<UpdateOptions>(t => t.IsUpsert == true),
    //                                                      It.IsAny<CancellationToken>()), Times.Once);
    //}

    [InlineData(6, 2, 2)]
    [InlineData(6, 1, 2)]
    [InlineData(5, 1, 1)]
    [InlineData(3, 1, 1)]
    [Theory]
    public async Task PagingData(int totalRecordsToMock, int currentPage, int expectedTotalPages)
    {
        var filterDef = Builders<Countries>.Filter.Where(predicate => predicate.Name == "USA");
        var sortDef = Builders<Countries>.Sort.Ascending(predicate => predicate.Name);

        var mockedData = Countries.MockData(totalRecordsToMock).ToList();

        MockIMongoCollection.Setup(x => x.CountDocumentsAsync(It.Is<FilterDefinition<Countries>>(t => t == filterDef), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<long>(mockedData.Count));

        MockIMongoCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Countries>>(), It.IsAny<FindOptions<Countries>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<IAsyncCursor<Countries>>(new MockAsyncCursor<Countries>(mockedData)));

        var result = await MockIMongoCollection.Object.FindAndPageItemsAsync(currentPage, 5, filterDef, sortDef);

        Assert.Equal(totalRecordsToMock, result.TotalRecords);
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }
}
