using Shop.Api.Entities;
using MongoDB.Driver;


namespace Shop.Api.Repositories{
    public class MongoDbRatingRepository : IRatingRepository{

        private const string databaseName = "shop";
        private const string collectionName = "Rate";

        private readonly IMongoCollection<RatingModel> ratingCollection;
        private readonly FilterDefinitionBuilder<RatingModel> filterBuilder = Builders<RatingModel>.Filter;
        private readonly IConfiguration configuration;

        //private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public MongoDbRatingRepository(IMongoClient mongoClient, IConfiguration configuration){
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            //database.DropCollection(collectionName);
            this.configuration = configuration;
            ratingCollection = database.GetCollection<RatingModel>(collectionName);
         
            //usersCollection.Indexes.DropAll();
            var Index = Builders<RatingModel>.IndexKeys.Ascending(indexKey => indexKey.ShopItemId).Ascending(indexKey => indexKey.UserId);
            ratingCollection.Indexes.CreateOne(new CreateIndexModel<RatingModel>(Index, new CreateIndexOptions { Unique = true }));
        }

        public async Task<Exception> PostRatingAsync(RatingModel rate){
            try{
                var filter =  filterBuilder.Where(existingRating => existingRating.UserId == rate.UserId && existingRating.ShopItemId == rate.ShopItemId);
                var rating = await ratingCollection.Find(filter).SingleOrDefaultAsync();

                if(rating==null){
                    await ratingCollection.InsertOneAsync(rate);
                }
                else{
                    var updater = Builders<RatingModel>.Update.Set("Rate", rate.Rate);
                    var update = await ratingCollection.UpdateOneAsync(filter, updater);
                }
                
                return null;
            }
            catch (Exception e){
                Console.WriteLine($"{e}");
                return e; 
            }
        }
        

        public async Task<Int64> UpdateRatingAsync(Guid userId, UpdateRate ratingUpdated)
        {
            var filter =  filterBuilder.Where(existingRating => 
                existingRating.UserId == userId && existingRating.ShopItemId == ratingUpdated.ShopItemId);
            var updater = Builders<RatingModel>.Update.Set("Rate", ratingUpdated.Rate);
            var update = await ratingCollection.UpdateOneAsync(filter, updater);

            return update.ModifiedCount;
        }

        public async Task<List<RatingModel>> GetAllYourRatingsAsync(Guid userId)
        {
            var filter =  filterBuilder.Where(existingRating => existingRating.UserId == userId);
            var rates =  await ratingCollection.Find(filter).ToListAsync();

            return rates;
        }
    } 
}