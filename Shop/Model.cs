using System.ComponentModel.DataAnnotations;
using Shop.Api.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

#nullable enable
namespace Shop.Api.Entities{

    public class ShopItem {
        public Guid Id {get; set;} 

        public string Name { get; set;} = string.Empty;

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set;}

        public string Description { get; set;} = string.Empty;

        public SeasonEnum Season {get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public string ImageFilePath {get; set;} = string.Empty;
    }

    public class ShopItemWithRatings : ShopItem{
        public double? AmountOfRatings {get; set;}
        public double? AverageRating {get; set;}
        public object Values {get; set;}
        public object Ratings {get; set;}
    }

    public class ShopItemDto {
        public Guid Id {get; set;}
        public string Name { get; set;} = string.Empty;
        public decimal Price { get; set;}
        public string Description { get; set;} = string.Empty;
        public SeasonEnum Season {get; set;}
        public DateTimeOffset CreatedDate { get; set;}
        public string ImageFilePath {get; set;} = string.Empty;
    }

    public class ShopItemWithRatingsDto: ShopItemDto{
        public double? AmountOfRatings {get; set;}
        public double? AverageRating {get; set;}
        public object Values {get; set;}
        public object Ratings {get; set;}
    }

    public record CreateItemDto
    {
        [Required] 
        public string Name { get; init;} = string.Empty; 
        
        [Required]
        public string Description { get; init;} = string.Empty; 
        
        [Range(1,1000)] 
        public decimal Price { get; init;}

        [Required]
        public SeasonEnum Season {get; set;}

        [Required]
        public IFormFile File {get; set;}

        
    };

    public record UpdateItemDto{
        [Required]
        public string Name { get; init;} = string.Empty;
        
        [Required]
        public string Description { get; init;} = string.Empty;
        
        [Required]
        [Range(1,1000)] 
        public decimal Price { get; init;}

        [Required]
        public SeasonEnum Season {get; set;}
    };

    public record FilterGetItem{
        [Required]
        public string? NameToMatch = string.Empty;
        
        [Required]
        public SeasonEnum? SeasonToMatch;
        
    };

    public class ShopItemWithAmount : ShopItem {
        public double? Amount;
    }

    public class ShopItemsDtoReturn{
        public ShopItemWithRatingsDto items;
        public long amountOfItem;
    }

    public record ShopItemsQuery {
        public string? nameToMatch { get; init;} = null;
        public SeasonEnum? seasonToMatch { get; init;} = null; 
        public int? limit { get; init;} = 10;
        public Boolean? sortAscending { get; init;} =  false;
        public int? skip { get; init;}  = 0;
    }

}