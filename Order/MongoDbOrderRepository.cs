using Shop.Api.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Shop.Api.Repositories{
    public class MongoDbOrderRepository : IOrderRepository{

        private const string databaseName = "shop";
        private const string collectionName = "Order";

        private readonly IMongoCollection<OrderModel> orderCollection;
        private readonly FilterDefinitionBuilder<OrderModel> filterBuilder = Builders<OrderModel>.Filter;
        private readonly IConfiguration configuration;

        //private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public MongoDbOrderRepository(IMongoClient mongoClient, IConfiguration configuration){
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            //database.DropCollection(collectionName);
            this.configuration = configuration;
            orderCollection = database.GetCollection<OrderModel>(collectionName);
         
            //usersCollection.Indexes.DropAll();
            //var Index = Builders<RatingModel>.IndexKeys.Ascending(indexKey => indexKey.ShopItemId).Ascending(indexKey => indexKey.UserId);
            //ratingCollection.Indexes.CreateOne(new CreateIndexModel<OrderModel>(Index, new CreateIndexOptions { Unique = true }));
        }

        public async Task<List<OrderModelWithItems>> GetOrdersAsync(Guid userId, Boolean OrderCompleted){
            var filter =  filterBuilder.Where(existingOrder => existingOrder.UserId == userId && existingOrder.Details.IsOrderCompleted == OrderCompleted 
                && existingOrder.TransactionConfirmed);
            
            var ordersBson = await orderCollection.
                Aggregate().
                Match(filter).
                Lookup("shopItems", "OrderedItems.ShopItemId", "_id",   "Items"). // BsonDocument After that line
                ToListAsync();

            var orders = ordersBson.Select(order=>BsonSerializer.Deserialize<OrderModelWithItems>(order)).ToList();    

            return (List<OrderModelWithItems>)orders;
        }
        

        public async Task CreateOrderAsync(OrderModel order)
        {
            await orderCollection.InsertOneAsync(order);
        }

        public async Task<Double> CompleteOrder(Guid OrderId, OrderDetails orderDetails)
        {
            var filter = filterBuilder.Where(existingOrder => existingOrder.Id == OrderId);

            var update = Builders<OrderModel>.Update.Set("Details", orderDetails);
            var updateOperation = await orderCollection.UpdateOneAsync(filter, update);

            return updateOperation.ModifiedCount;
        }

        public async Task<OrderModelWithItems> GetOrderAsync(Guid userId, Guid orderId){
            var filter =  filterBuilder.Where(existingOrder => (existingOrder.UserId == userId && existingOrder.Id == orderId));
           
            var orderBson = await orderCollection.Aggregate().
                Match(filter).
                Lookup("shopItems", "OrderedItems.ShopItemId", "_id",   "Items").
                SingleOrDefaultAsync();

            var order = BsonSerializer.Deserialize<OrderModelWithItems>(orderBson);

            return order;
        }

        public async Task ConfirmPaymentAsync(string paymentId){
            var filter =  filterBuilder.Where(existingOrder => existingOrder.TransactionId == paymentId);
            var update = Builders<OrderModel>.Update.Set("TransactionConfirmed", true);
            var confirmPayment = await orderCollection.UpdateOneAsync(filter, update);
            return;
        }

        
    } 
}