using System.ComponentModel.DataAnnotations;
using Shop.Api.Enums;
#nullable enable
namespace Shop.Api.Entities{

    public class RatingModel {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public Guid ShopItemId {get; set;}

        [Range(1,5)] 
        public Int16 Rate {get; set;}

        public DateTimeOffset CreatedDate { get; set;}
    }

    public class RatingDto {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public Guid ShopItemId {get; set;}

        [Range(1,5)] 
        public Int16 Rate {get; set;}

        public DateTimeOffset CreatedDate { get; set;}
    }

    public class UpdateRate {
        [Required]
        public Guid ShopItemId {get; set;}

        [Required]
        [Range(1,5)] 
        public Int16 Rate {get; set;}
    }

    public class CreateRate {
        [Required]
        [Range(1,5)] 
        public Int16 Rate {get; set;}
    }
}