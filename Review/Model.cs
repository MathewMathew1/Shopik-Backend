using System.ComponentModel.DataAnnotations;
using Shop.Api.Enums;
#nullable enable
namespace Shop.Api.Entities{

    public class ReviewModel {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public Guid ShopItemId {get; set;}

        public String ReviewText {get; set;}

        public DateTimeOffset CreatedDate { get; set;}
    }

    public class ReviewModelDto {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public Guid ShopItemId {get; set;}

        public String ReviewText {get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public RateFromUserInfo  RateFromUser { get; set;}

        public UserSendingInfo  UserSending {get; set;}
    }

    public class ReviewModelWithUserAndRateInList {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public Guid ShopItemId {get; set;}

        public String ReviewText {get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public List<RatingModel> RateFromUser { get; set;}

        public List<UserModel> UserSending {get; set;}
    }

    public class RateFromUserInfo {
        [Range(1,5)] 
        public Int16 Rate {get; set;}

        public DateTimeOffset CreatedDate { get; set;}
    }

    public class UserSendingInfo {

        public string Username { get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public RoleEnum Role {get; set;}
    }


    public class ReviewModelWithUserAndRate {
        public Guid Id {get; set;}

        public Guid UserId {get; set;}

        public Guid ShopItemId {get; set;}

        public String ReviewText {get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public RatingModel RateFromUser { get; set;}

        public UserModel UserSending {get; set;}
    }

    public class UpdateReview {
       [Required]
        [StringLength(1000, MinimumLength = 8, ErrorMessage = "Minimal Length")]
        public String ReviewText {get; set;}
    }

    public class CreateReview {
        [Required]
        [StringLength(1000, MinimumLength = 8, ErrorMessage = "Minimal Length")]
        public String ReviewText {get; set;}
    }
}