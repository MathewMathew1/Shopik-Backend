using Shop.Api.Entities;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Shop.Api.Repositories{
    public class MongoDbReviewRepository : IReviewRepository{

        private const string databaseName = "shop";
        private const string collectionName = "Review";

        private readonly IMongoCollection<ReviewModel> reviewCollection;
        private readonly FilterDefinitionBuilder<ReviewModel> filterBuilder = Builders<ReviewModel>.Filter;
        private readonly IConfiguration configuration;

        //private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public MongoDbReviewRepository(IMongoClient mongoClient, IConfiguration configuration){
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            //database.DropCollection(collectionName);
            this.configuration = configuration;
            reviewCollection = database.GetCollection<ReviewModel>(collectionName);
         
            //usersCollection.Indexes.DropAll();
            var Index = Builders<ReviewModel>.IndexKeys.Ascending(indexKey => indexKey.ShopItemId).Ascending(indexKey => indexKey.UserId);
            reviewCollection.Indexes.CreateOne(new CreateIndexModel<ReviewModel>(Index, new CreateIndexOptions { Unique = true }));
        }

        public async Task<Exception> PostReviewAsync(ReviewModel review){
            try{
                await reviewCollection.InsertOneAsync(review);
                return null;
            }
            catch (Exception e){
                Console.WriteLine($"{e}");
                return e; 
            }
        }
        
        public async Task<IEnumerable<ReviewModelDto>> GetReviewsAsync(Guid shopItemId)
        {
            try{
                var filter =  filterBuilder.Eq(existingReview => existingReview.ShopItemId, shopItemId);

                BsonArray subPipeline = new BsonArray();

                subPipeline.Add(
                    new BsonDocument("$match",new BsonDocument(
                        "$expr", new BsonDocument(
                        "$and", new BsonArray { 
                                new BsonDocument("$eq", new BsonArray{"$UserId", "$$reviewerId"} ), 
                                new BsonDocument("$eq", new BsonArray{"$ShopItemId", "$$reviewedItemId"} )
                            }  
                        )
                    ))
                );
                var lookup = new BsonDocument("$lookup", 
                    new BsonDocument("from", "Rate")
                        .Add("let", new BsonDocument("reviewerId", "$UserId").Add("reviewedItemId", "$ShopItemId"))
                        .Add("pipeline", subPipeline)
                        .Add("as", "RateFromUser")
                );

                var set = new BsonDocument("$set", 
                    new BsonDocument("UserSending", new BsonDocument("$first", "$UserSending"))
                        .Add("RateFromUser", new BsonDocument("$first", "$RateFromUser"))
                    
                );


                ProjectionDefinition<ReviewModelWithUserAndRate> projection = new BsonDocument
                {
  
                    {new BsonDocument("UserSending._id", 0)},
                    {new BsonDocument("UserSending.Password", 0)},
                    {new BsonDocument("RateFromUser.UserId", 0)},
                    {new BsonDocument("RateFromUser._id", 0)},
                    {new BsonDocument("RateFromUser.ShopItemId", 0)},
                };


                var reviews = await reviewCollection.Aggregate().Match(filter).
                    Lookup("User", "UserId", "_id", "UserSending").
                    AppendStage<ReviewModelWithUserAndRateInList>(lookup).
                    AppendStage<ReviewModelWithUserAndRate>(set).
                    Project<ReviewModelDto>(projection).
                    ToListAsync(); 

                return (IEnumerable<ReviewModelDto>)reviews;
            }
            catch (Exception e){
                Console.WriteLine($"{e}");
                return null; 
            }
        }

        public async Task<Int64> DeleteReviewAsync(Guid id)
        {
            var filter =  filterBuilder.Eq(existingReview => existingReview.Id, id);
            var delete = await reviewCollection.DeleteOneAsync(filter);
            
            return delete.DeletedCount;
        }

        public async Task<Int64> UpdateReviewAsync(Guid id, ReviewModel reviewUpdated)
        {
            
            var filter =  filterBuilder.Where(existingReview => existingReview.Id == id);
            var update = await reviewCollection.ReplaceOneAsync(filter, reviewUpdated);

            return update.ModifiedCount;
        }

        public async Task<ReviewModel> GetReviewAsync(Guid id)
        {
            
            var filter =  filterBuilder.Where(existingReview => existingReview.Id == id);
            var review = await reviewCollection.Find(filter).SingleOrDefaultAsync();;

            return review;
        }


    } 
}