using System.ComponentModel.DataAnnotations;
using Shop.Api.Enums;
#nullable enable
namespace Shop.Api.Entities{

    public class OrderModel {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public List<OrderedItem> OrderedItems {get; set;}

        public OrderDetails Details {get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public String Address {get; set;}

        public String Country {get; set;}
        
        public String City {get; set;}

        public String PostalCode {get; set;}

        public String Region {get; set;}

        public String TransactionId {get; set;}

        public Boolean TransactionConfirmed {get; set;}

        public DateTimeOffset ExpectedDeliveryTime { get; set;}
    }

    public class OrderModelWithItems: OrderModel {

        public List<ShopItem> Items {get; set;}

    }

    public class OrderModeWithItemsDto: OrderModelDto {

        public List<ShopItem> Items {get; set;}

    }

    public class OrderModelDto {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public List<OrderedItem> OrderedItems {get; set;}

        public OrderDetails Details {get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public String Address {get; set;}

        public String Country {get; set;}
        
        public String City {get; set;}

        public String PostalCode {get; set;}

        public String Region {get; set;}

        public DateTimeOffset ExpectedDeliveryTime { get; set;}

        public Boolean TransactionConfirmed {get; set;}
    }

    public class OrderedItem {
        [Required]
        public Guid ShopItemId {get; set;}

        [Required]
        [Range(1,99)] 
        public double Amount {get; set;}
    }

    public class AmountPaypal {
        [Required]
        public Guid ShopItemId {get; set;}

        [Required]
        [Range(1,99)] 
        public double Amount {get; set;}
    }

    public class OrderDetails {
        
        public Boolean IsOrderCompleted {get; set;}

        public Guid? WorkerConfirmingDeliveryId {get; set;}

        public DateTimeOffset? CompleteDate { get; set;}
    }

    public record CreateOrderDto
    {
        [Required, Url] 
        public string RedirectUrlSuccess {get; set;}

        [Required, Url] 
        public string RedirectUrlFailure {get; set;}

        [Required] 
        public List<OrderedItem> OrderedItems {get; set;}
        
        [Required]
        public String Address {get; set;} 

        [Required, MinLength(2)]
        public String Country {get; set;}

        [Required]
        public String Region {get; set;}

        [Required, Range(3,12)]
        public String PostalCode {get; set;}

        [Required, MinLength(2)]
        public String City {get; set;}
    };


}