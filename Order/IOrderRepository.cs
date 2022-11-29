using Shop.Api.Entities;

namespace Shop.Api.Repositories {
    public interface IOrderRepository{
        Task<List<OrderModelWithItems>> GetOrdersAsync(Guid userId, Boolean OrderCompleted);
        Task CreateOrderAsync(OrderModel order);
        Task<Double> CompleteOrder(Guid OrderId, OrderDetails orderDetails);
        Task<OrderModelWithItems> GetOrderAsync(Guid userId, Guid orderId);
        Task ConfirmPaymentAsync(string paymentId);
    }
}