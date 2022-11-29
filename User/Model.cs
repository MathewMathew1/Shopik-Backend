using System.ComponentModel.DataAnnotations;
using Shop.Api.Enums;
#nullable enable
namespace Shop.Api.Entities{

    public class UserSignUp {
        public string Username { get; set;}

        [StringLength(120, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 120 characters")]
        public string Password { get; set;}
    }

    public class UserLogin {
        public string Username { get; set;}

        public string Password { get; set;}
    }

    public class UserModel {
        public Guid Id {get; set;} // Guid To tak jak UUID innaczej

        public string Username { get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public string Password { get; set;}

        public RoleEnum Role {get; set;}
    }

    public class UserModelDto {
        public Guid Id {get; set;} // Guid To tak jak UUID innaczej

        public string Username { get; set;}

        public DateTimeOffset CreatedDate { get; set;}

        public RoleEnum Role {get; set;}
    }

    public class AuthenticateData {
        public UserModel User {get; set;} // Guid To tak jak UUID innaczej

        public string Token { get; set;}
    }

    public class AuthenticateDataDto {
        public UserModelDto User {get; set;} // Guid To tak jak UUID innaczej

        public string AccessToken { get; set;}
    }

    public class ChangeUserRoleDto {
        [Required]
        public RoleEnum newRole {get; set;} // Guid To tak jak UUID innaczej
    }
}