using AutoMapper;
using Shop.Api.Entities;

namespace Shop.Api.Mappers 
{
  public class OrderMapper: Profile 
  {  
    public OrderMapper()
    {
       CreateMap<OrderModel, OrderModelDto>();
       CreateMap<OrderModelWithItems, OrderModeWithItemsDto>();
    }
  }
}