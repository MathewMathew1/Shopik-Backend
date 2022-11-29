using Shop.Api.Entities;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Shop.Api.Repositories{
    public class MongoDbShopItemsRepository : IShopItemRepository{

        private const string databaseName = "shop";
        private const string collectionName = "shopItems";

        private readonly IMongoCollection<ShopItem> itemsCollection;
        private readonly FilterDefinitionBuilder<ShopItem> filterBuilder = Builders<ShopItem>.Filter;
        //private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public MongoDbShopItemsRepository(IMongoClient mongoClient){
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            
            itemsCollection = database.GetCollection<ShopItem>(collectionName);
            var Index = Builders<ShopItem>.IndexKeys.Ascending(indexKey => indexKey.Price);
            itemsCollection.Indexes.CreateOne(new CreateIndexModel<ShopItem>(Index));

            var items = itemsCollection.Find(_=>0==0).ToList();

            items.ForEach((item)=>{
                itemsCollection.ReplaceOne(x=>x.Id==item.Id, item);
            });
            
        }

        public async Task<ShopItemWithRatings> GetShopItemAsync(Guid id){
            var filter = filterBuilder.Eq(item => item.Id, id);
            ProjectionDefinition<BsonDocument> projection = new BsonDocument
            {
                {
                    "AmountOfRatings",
                    new BsonDocument("$size", "$Ratings")
                },
                {
                    "AverageRating",
                    new BsonDocument("$avg", "$Ratings.Rate")
                },
                {
                    "Values",
                    new BsonDocument("$size", new BsonDocument("$setDifference", new BsonArray {"$Ratings.Rate", new BsonArray{}}))
                },
                {new BsonDocument("totalCount", 1)},
                {new BsonDocument("Id", 1)},
                {new BsonDocument("Name", 1)},
                {new BsonDocument("Price", 1)},
                {new BsonDocument("Description", 1)},
                {new BsonDocument("Season", 1)},
                {new BsonDocument("CreatedDate", 1)},
                {new BsonDocument("ImageFilePath", 1)},
            };

        

            var unwind = new BsonDocument("$unwind", "$Ratings");
            var lookup = new BsonDocument("$group", 
                    new BsonDocument("_id", "Ratings.Rate")
            .Add("totalCount", new BsonDocument("$sum", "1")));
            
            var item = await itemsCollection.
                Aggregate().
                Match(filter).
                Lookup("Rate", "Id", "ShopItemId", "Ratings"). // BsonDocument After that line
                Project<ShopItemWithRatings>(projection).
                SingleOrDefaultAsync();
           
           return item;
        }

        public async Task<(List<ShopItemWithRatings>, long)> GetShopItemsAsync(FilterGetItem filterOptions, int limit, Boolean sortAscending, int skip)
        {
            FilterDefinition<ShopItem>? filter = filterBuilder.Empty;

            if(filterOptions.NameToMatch!=null){ 
                var nameFilter = filterBuilder.Where(item=>item.Name.Contains(filterOptions.NameToMatch));
                filter &= nameFilter;
            }
            if(filterOptions.SeasonToMatch!=null){ 
                var seasonFilter = filterBuilder.Where(item=>item.Season==filterOptions.SeasonToMatch);
                filter &= seasonFilter;
            }

            ProjectionDefinition<BsonDocument> projection = new BsonDocument
            {
                {
                    "AmountOfRatings",
                    new BsonDocument("$size", "$Ratings")
                },
                {
                    "AverageRating",
                    new BsonDocument("$avg", "$Ratings.Rate")
                },
               
                {new BsonDocument("Id", 1)},
                {new BsonDocument("Name", 1)},
                {new BsonDocument("Price", 1)},
                {new BsonDocument("Description", 1)},
                {new BsonDocument("Season", 1)},
                {new BsonDocument("CreatedDate", 1)},
                {new BsonDocument("ImageFilePath", 1)},
            };

            SortDefinition<ShopItem> sort;
            if(sortAscending)sort = Builders<ShopItem>.Sort.Ascending(a => a.Price);
            else sort = Builders<ShopItem>.Sort.Descending(a => a.Price);

            var countItems = await itemsCollection.CountDocumentsAsync(filter);
            var items = await itemsCollection.
                Aggregate().
                Match(filter).
                Sort(sort).
                Lookup("Rate", "Id", "ShopItemId", "Ratings").
                 // BsonDocument After that line
                Project<ShopItemWithRatings>(projection).
                
                Skip(skip).
                Limit(limit).
                ToListAsync();

            return (items, countItems);
        }

        public async Task CreateShopItemAsync(ShopItem item){
            await itemsCollection.InsertOneAsync(item);
        }

        public async Task UpdateShopItemAsync(ShopItem item){
            var filter =  filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
            await itemsCollection.ReplaceOneAsync(filter, item); 
        }

        public async Task DeleteShopItemAsync(Guid id){
            var filter =  filterBuilder.Eq(existingItem => existingItem.Id, id);
            await itemsCollection.DeleteOneAsync(filter);
        }
    } 
}