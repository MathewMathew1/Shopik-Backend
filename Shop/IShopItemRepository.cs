using Shop.Api.Entities;

namespace Shop.Api.Repositories {
    public interface IShopItemRepository{
        Task<ShopItemWithRatings> GetShopItemAsync(Guid id);
        Task<(List<ShopItemWithRatings>, long)> GetShopItemsAsync(FilterGetItem filterOptions, int limit, Boolean sortAscending, int skip);
        Task CreateShopItemAsync(ShopItem item);
        Task UpdateShopItemAsync(ShopItem item);
        Task DeleteShopItemAsync(Guid id);
    }
}