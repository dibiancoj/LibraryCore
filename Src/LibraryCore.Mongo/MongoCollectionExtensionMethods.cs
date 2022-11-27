using LibraryCore.Core.Paging;
using MongoDB.Driver;

namespace LibraryCore.Mongo;

//var dbClient = new MongoClient("mongodb://root:Pass!word@localhost:27017");
//var database = dbClient.GetDatabase("JasonTest");
//var collection = database.GetCollection<College>("College");
//var upsertData = Enumerable.Range(0, 1000).Select(x => new College
//{
//    Id = ObjectId.GenerateNewId(),
//    Name = "Boston University",
//    State = "MA"
//}).ToList();
//var upsertResult = await collection.BulkWriteAsync(upsertData.Select(x => new InsertOneModel<College>(x)).ToList());

public static class MongoCollectionExtensionMethods
{
    //don't really find this abstracted enough to make it worth while to add.
    //public static async Task<UpsertResponse> UpsertOneAsync<TModel>(this IMongoCollection<TModel> mongoCollection,
    //                                                                ObjectId id,
    //                                                                FilterDefinition<TModel> filterExpression,
    //                                                                UpdateDefinition<TModel> updateExpression)
    //{
    //    //id == null ? ObjectId.GenerateNewId() : new ObjectId(id);

    //    //Builders<SampleCollection>.Filter.Where(predicate => predicate.Id == objectId),
    //    //                                                    Builders<SampleCollection>.Update
    //    //                                                        .SetOnInsert(predicate => predicate.CreatedDate, now)
    //    //                                                        .Set(predicate => predicate.LastUpdatedDate, now)
    //    //                                                        .Set(predicate => predicate.FirstName, input.FirstName)
    //    //                                                        .Set(predicate => predicate.LastName, input.LastName)

    //    var result = await mongoCollection.UpdateOneAsync(filterExpression,
    //                                                      updateExpression,
    //                                                      new UpdateOptions { IsUpsert = true });

    //    return new UpsertResponse(id, result.MatchedCount > 0 ? UpsertModelEnum.Updated : UpsertModelEnum.Inserted);
    //}

    public record PagedData<T>(long TotalRecords, int TotalPages, int CurrentPage, IEnumerable<T> Data);

    public static async Task<PagedData<T>> FindAndPageItemsAsync<T>(this IMongoCollection<T> mongoCollection,
                                                                       int currentPage,
                                                                       int pageSize,
                                                                       FilterDefinition<T> filterExpression,
                                                                       SortDefinition<T> sortExpression)
    {
        //general note this will all suffer from an N+1 as the dataset grows. 500k will take 120ms vs 35ms.
        //we would need to create a stateful api to deal with this which is not needed at this time.
        //ie: Find(x => x.FormType == 'MyForm' && x._id > someid).Sort
        //the db needs to skip over a ton of records which it needs to calculate and can't be index with this patter

        //with a larger dataset - the 2 query approach is much faster 130ms vs 50ms
        var countTask = mongoCollection.CountDocumentsAsync(filterExpression);

        var dataTask = mongoCollection.Find(filterExpression)
            .Sort(sortExpression)
            .Skip((currentPage - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        var count = await countTask;

        return new PagedData<T>(count, Pagination.CalculateTotalPages((int)count, pageSize), currentPage, await dataTask);

        //this method below is 1 call to mongo...but is slower in alot of scenarios. The one above is faster but results in the entire collection being scanned which results in slower performance for the more records you have.

        //var countFacet = AggregateFacet.Create("Count",
        //       PipelineDefinition<T, AggregateCountResult>.Create(new[]
        //       {
        //            PipelineStageDefinitionBuilder.Count<T>()
        //       }));

        //var dataFacet = AggregateFacet.Create("Data",
        //    PipelineDefinition<T, T>.Create(new[]
        //    {
        //        PipelineStageDefinitionBuilder.Sort(sortExpression),
        //        PipelineStageDefinitionBuilder.Skip<T>((currentPage - 1) * pageSize),
        //        PipelineStageDefinitionBuilder.Limit<T>(pageSize),
        //    }));

        //var aggregation = await mongoCollection.Aggregate()
        //     .Match(filterExpression)
        //     .Facet(countFacet, dataFacet)
        //     .ToListAsync();

        //var count = aggregation.First()
        //    .Facets.First(x => x.Name == "Count")
        //    .Output<AggregateCountResult>()
        //    .FirstOrDefault()?.Count ?? 0;

        //var data = aggregation.First()
        //    .Facets.FirstOrDefault(x => x.Name == "Data")
        //    ?.Output<T>() ?? Enumerable.Empty<T>();

        //return new PagedData<T>(count, MiscUtility.CalculateTotalPages(count, pageSize), currentPage,  data);
    }

}
