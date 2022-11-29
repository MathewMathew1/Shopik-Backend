using Shop.Api.Entities;

namespace Shop.Api.Repositories {
    public interface IRatingRepository{
        Task<Exception> PostRatingAsync(RatingModel rate);
        Task<Int64> UpdateRatingAsync(Guid userId, UpdateRate ratingUpdated);
        Task<List<RatingModel>> GetAllYourRatingsAsync(Guid userId);
    }
}