using Shop.Api.Entities;
using Shop.Api.Repositories;
using Shop.Api.Enums;
using MongoDB.Driver;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Shop.Api.Repositories{
    public class MongoDbUserRepository : IUserRepository{

        private const string databaseName = "shop";
        private const string collectionName = "User";

        private readonly IMongoCollection<UserModel> usersCollection;
        private readonly FilterDefinitionBuilder<UserModel> filterBuilder = Builders<UserModel>.Filter;
        private readonly IConfiguration configuration;

        //private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        public MongoDbUserRepository(IMongoClient mongoClient, IConfiguration configuration){
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            this.configuration = configuration;
            usersCollection = database.GetCollection<UserModel>(collectionName);
            
            //usersCollection.Indexes.DropAll();
            var Index = Builders<UserModel>.IndexKeys.Ascending(indexKey => indexKey.Username);
            usersCollection.Indexes.CreateOne(new CreateIndexModel<UserModel>(Index, new CreateIndexOptions { Unique = true }));
            var update = Builders<UserModel>.Update.Set("Role", RoleEnum.SuperAdmin);
            //Guid guid = new Guid("491783e7-efea-4b12-9ed0-7b44fb3acf34");
            //var filter =  filterBuilder.Eq(existingUser => existingUser.Id, guid);
           // var updateOperation = usersCollection.UpdateOne(filter, update);
            //var users = usersCollection.Find(_ => true).ToList();
            
        }

        public async Task<Exception> SignUp(UserModel userModel){
            try{
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(userModel.Password);
                userModel.Password = passwordHash;
                await usersCollection.InsertOneAsync(userModel);
                return null;
            }
            catch (Exception e){
                Console.WriteLine($"{e}");
                return e; 
            }
        }
        
        public async Task<AuthenticateData> Login(UserLogin userLogin)
        {
            var filter =  filterBuilder.Eq(existingUser => existingUser.Username, userLogin.Username);
            var user = await usersCollection.Find(filter).SingleOrDefaultAsync();

            if(user==null) return null;
            bool correctPassword = BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password);
            if(!correctPassword) return null;

            var token = generateJwtToken(user);

            var authenticateData = new AuthenticateData(){
                User=user,
                Token=token
            };
            return authenticateData;
        }

        public async Task<UserModel> GetUserById(Guid id)
        {
            var filter =  filterBuilder.Eq(existingUser => existingUser.Id, id);
            var user = await usersCollection.Find(filter).SingleOrDefaultAsync();

            return user;
        }

        private string generateJwtToken(UserModel user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();

            string secret = configuration.GetSection("TokenSettings:Secret").Value;
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("id", user.Id.ToString()),
                    new Claim("role", user.Role.ToString()) 
                }),
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<Double> ChangeUserRole(Guid userId, RoleEnum newRole, RoleEnum roleOfChangingRoleUser)
        {
            FilterDefinition<UserModel>? filter = filterBuilder.Empty;

            if(roleOfChangingRoleUser==RoleEnum.SuperAdmin){ 
                var roleFilter = filterBuilder.Where(user=> user.Id==userId && user.Role!=RoleEnum.SuperAdmin);
                filter &= roleFilter;
            }
            else{ 
                var roleFilter = filterBuilder.Where(user=> user.Id==userId && user.Role!=RoleEnum.SuperAdmin && user.Role!=RoleEnum.Admin);
                filter &= roleFilter;
            }

            var update = Builders<UserModel>.Update.Set("Role", newRole);

            var updateOperation = await usersCollection.UpdateOneAsync(filter, update);

            return updateOperation.ModifiedCount;    
        }

    } 
}