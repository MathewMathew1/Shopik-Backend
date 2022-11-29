using Microsoft.AspNetCore.Mvc;
using Shop.Api.Entities;
using Shop.Api.Repositories;
using AutoMapper;
using Shop.Api.MiddleWare;
using Shop.Api.Enums;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/v1/rating")]
public class RatingController : ControllerBase
{
    private readonly IRatingRepository repository;
    private readonly ILogger<RatingController> logger;
    private IMapper mapper;

    public RatingController(ILogger<RatingController> logger, IMapper mapper, IRatingRepository repository)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.repository = repository;
    }

    [Authorize]
    [HttpPost("shopItem/{id}")]
    public async Task<ActionResult> CreateRateAsync(Guid id, [FromBody] CreateRate createRate)
    {       
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

            RatingModel rate = new(){
                Id = Guid.NewGuid(),
                ShopItemId = id,
                Rate = createRate.Rate,
                UserId = user.Id,
                CreatedDate = DateTimeOffset.UtcNow,
            }; 

            var ratingCreated = await repository.PostRatingAsync(rate);

            if(ratingCreated==null) return Ok();
            
            return BadRequest(new {error = "You have already rated this shop item" });
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [Authorize]
    [HttpPatch]
    public async Task<ActionResult<IEnumerable<ReviewModel>>> UpdateReviewsAsync([FromBody] UpdateRate updatedRating)
    {       
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;
            
            var ratingUpdate = await repository.UpdateRatingAsync(user.Id, updatedRating);

            if(ratingUpdate>0) return NoContent();
            
            return NotFound(new{error = "Rating not found"});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

}