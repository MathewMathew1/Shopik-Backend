using Shop.Api.Entities;

namespace Shop.Api.Storage {
    public interface IImageStorage{
        Task<String> PostImage(IFormFile File);
    }
}