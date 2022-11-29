using Microsoft.AspNetCore.Mvc;
using Shop.Api.Entities;
using Shop.Api.Repositories;
using AutoMapper;
using Shop.Api.Enums;
using Shop.Api.MiddleWare;
using Shop.Api.Storage;
//using Shop.Api.MiddleWare;


namespace Shop.Api.Controllers;

[ApiController]
[Route("api/v1/shopItems")]
public class ShopItemsController : ControllerBase
{
    private readonly IShopItemRepository repository;
    private readonly ILogger<ShopItemsController> logger;
    private IMapper mapper;
    private readonly IConfiguration configuration;
    private readonly IImageStorage imageStorage;

    public ShopItemsController(ILogger<ShopItemsController> logger, IMapper mapper, IShopItemRepository repository, 
        IImageStorage imageStorage)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.repository = repository;
        this.imageStorage = imageStorage;
    }

    [HttpGet]
    public async Task<ActionResult<ShopItemsDtoReturn>> GetShopItemsAsync([FromQuery] ShopItemsQuery query)
    {       
        try{
   
            FilterGetItem filterObject = new FilterGetItem{
                NameToMatch = query.nameToMatch,
                SeasonToMatch = query.seasonToMatch
            };

            var result = await repository.GetShopItemsAsync(filterObject, query.limit.Value, query.sortAscending.Value, query.skip.Value);
            
            var items = result.Item1.Select(item => 
                {
                    return this.mapper.Map<ShopItemWithRatingsDto>(item);
                }
            );
            

            return Ok(new { items = items.ToList(), amountOfItems = result.Item2 });
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ShopItemDto>> CreateItemAsync([FromForm] CreateItemDto createShopItemDto){
        try{

            var imagePath = await this.imageStorage.PostImage(createShopItemDto.File);

            if(imagePath==null) return StatusCode(500, new {error = "unable to process request"});

            ShopItem shopItem = new(){
                Id = Guid.NewGuid(),
                Name = createShopItemDto.Name,
                Description = createShopItemDto.Description,
                Price = createShopItemDto.Price,
                Season = createShopItemDto.Season,
                CreatedDate = DateTimeOffset.UtcNow,
                ImageFilePath = imagePath
            };

            await repository.CreateShopItemAsync(shopItem);

            return CreatedAtAction(nameof(GetShopItemsAsync), new {id = shopItem.Id}, this.mapper.Map<ShopItemDto>(shopItem));
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        }
        
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShopItemWithRatingsDto>> GetShopItemAsync(Guid id){ //Actionresult po to by moc zwrocic cos innego niz item
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"];
            var item = await repository.GetShopItemAsync(id);

            if(item is null){
                return NotFound(new {error = "Resource couldn't be found"}); 
            }
            
            return Ok(new{item = this.mapper.Map<ShopItemWithRatingsDto>(item) });
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        }   
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto itemDto){
        try{
            var existingItem = await repository.GetShopItemAsync(id);

            if(existingItem is null){
                return NotFound(new {error = "Resource couldn't be found"});
            }
           
            existingItem.Name = itemDto.Name;
            existingItem.Price = itemDto.Price;
            existingItem.Description = itemDto.Description;
            existingItem.Season = itemDto.Season;

            await repository.UpdateShopItemAsync(existingItem);

            return NoContent();
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteShopItemAsync(Guid id){
        try{
            var existingItem = repository.GetShopItemAsync(id);

            if(existingItem is null){
                return NotFound(new{error = "Resource couldn't be found"});
            }

            await repository.DeleteShopItemAsync(id);

            return NoContent();
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        }
    }       
}
