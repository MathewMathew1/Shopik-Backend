using AutoMapper;
using Shop.Api.Entities;

namespace Shop.Api.Mappers 
{
  public class ShopItemMapper: Profile 
  {  
    public ShopItemMapper()
    {
       CreateMap<ShopItem, ShopItemDto>();
       CreateMap<ShopItemWithRatings, ShopItemWithRatingsDto>();
       CreateMap<UserModel, UserModelDto>();
    }
  }
}