using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Net.Mime;
using System.Text.Json;
using Shop.Api.Repositories;
using MongoDB.Driver;
using Shop.Api.Settings;
using Shop.Api.MiddleWare;
using System.Text.Json.Serialization;
using Shop.Api.Storage;
using Shop.Api.Payments;
//using Shop.Api.MiddleWare;

var builder = WebApplication.CreateBuilder(args);

IConfigurationBuilder configB = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(true);

IConfiguration config = configB.Build();

// Add services to the container
var mongoDbSettings = config.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

builder.Services.AddSingleton<IMongoClient>(ServiceProvider => {
    return  new MongoClient(mongoDbSettings.ConnectionString);
});
builder.Services.AddSingleton<IShopItemRepository, MongoDbShopItemsRepository>();
builder.Services.AddSingleton<IUserRepository, MongoDbUserRepository>();
builder.Services.AddSingleton<IReviewRepository, MongoDbReviewRepository>();
builder.Services.AddSingleton<IRatingRepository, MongoDbRatingRepository>();
builder.Services.AddSingleton<IOrderRepository, MongoDbOrderRepository>();
builder.Services.AddSingleton<IImageStorage, IgmurImageStorage>();
builder.Services.AddSingleton<IPaymentProcessor, StripePayment>();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(opt =>
     {
         opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
     })
;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
    {
        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
    }));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHealthChecks()
.AddMongoDb(
    mongoDbSettings.ConnectionString, 
    name: "mongodb", 
    timeout: TimeSpan.FromSeconds(3),
    tags: new[] {"ready"}
);
Console.Write("Started server");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.UseAuthentication();
app.UseCors("corsapp");

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health/ready", new HealthCheckOptions{
    
    Predicate = (check) => check.Tags.Contains("ready"),
    ResponseWriter = async(context, report) => {
        Console.Write(report.Status.ToString());
        var result = JsonSerializer.Serialize(
            new{
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception != null ? entry.Value.Exception.Message: "none",
                    duration = entry.Value.Duration.ToString()
                })
            }
        );
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/live", new HealthCheckOptions{
    Predicate = (_) => false
});

app.Run();
