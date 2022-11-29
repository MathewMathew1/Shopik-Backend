using Microsoft.AspNetCore.Mvc;
using Shop.Api.Entities;
using Shop.Api.Repositories;
using AutoMapper;
using Shop.Api.MiddleWare;
using Shop.Api.Enums;

namespace Shop.Api.Controllers;

[ApiController]
[Route("api/v1/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IReviewRepository repository;
    private readonly ILogger<ReviewController> logger;
    private IMapper mapper;

    public ReviewController(ILogger<ReviewController> logger, IMapper mapper, IReviewRepository repository)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.repository = repository;
    }

    [Authorize]
    [HttpPost("shopItem/{id}")]
    public async Task<ActionResult<ReviewModel>> CreateReviewAsync(Guid id,[FromBody] CreateReview createReview)
    {       
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

            ReviewModel review = new(){
                Id = Guid.NewGuid(),
                ShopItemId = id,
                ReviewText = createReview.ReviewText,
                UserId = user.Id,
                CreatedDate = DateTimeOffset.UtcNow,
            }; 

            var reviewCreated = await repository.PostReviewAsync(review);

            if(reviewCreated==null) return Ok(new{review = review});
            
            return BadRequest(new{error = "You have already posted review on this shop item"});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [HttpGet("shopItem/{shopItemId}")]
    public async Task<ActionResult<IEnumerable<ReviewModelDto>>> GetReviewsAsync(Guid shopItemId)
    {       
        try{
            var reviews = await repository.GetReviewsAsync(shopItemId);

            if(reviews==null) return NotFound();
            
            return Ok(new {reviews = reviews.ToList()});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500);
        } 
    }

    [Authorize]
    [HttpPatch("{reviewId}")]
    public async Task<ActionResult<IEnumerable<ReviewModel>>> UpdateReviewsAsync(Guid reviewId, [FromBody] UpdateReview updatedReview)
    {       
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

            var review = await repository.GetReviewAsync(reviewId);

            if(review==null) return NotFound();

            if(review.UserId!=user.Id) return StatusCode(403);
            
            review.ReviewText = updatedReview.ReviewText;
            var updateReview = await repository.UpdateReviewAsync(reviewId, review);

            if(updateReview>0) return NoContent();
            
            return StatusCode(500);
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

    [Authorize]
    [HttpDelete("{reviewId}")]
    public async Task<ActionResult<IEnumerable<ReviewModel>>> DeleteReviewsAsync(Guid reviewId)
    {       
        try{
            UserModel user = (UserModel)Request.HttpContext.Items["User"]!;

            var review = await repository.GetReviewAsync(reviewId);

            if(review==null) return NotFound(new{error = "Resource couldn't be found"});

            if(review.UserId!=user.Id && user.Role!=RoleEnum.Admin) return StatusCode(403, new{error = "Resource cannot be deleted with your privileges"});

            var deleteReview = await repository.DeleteReviewAsync(reviewId);

            if(deleteReview>0) return NoContent();
            
            return NotFound(new{error = "Resource couldn't be found"});
        }
        catch (Exception e){
            logger.LogError($"{e}");
            return StatusCode(500, new {error = "Unexpected error try again"});
        } 
    }

}