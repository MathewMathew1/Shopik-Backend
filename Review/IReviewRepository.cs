using Shop.Api.Entities;

namespace Shop.Api.Repositories {
    public interface IReviewRepository{
        Task<IEnumerable<ReviewModelDto>> GetReviewsAsync(Guid shopItemId);
        Task<Exception> PostReviewAsync(ReviewModel review);
        Task<Int64> UpdateReviewAsync(Guid id, ReviewModel reviewUpdated);
        Task<Int64> DeleteReviewAsync(Guid id);
        Task<ReviewModel> GetReviewAsync(Guid id);
    }
}