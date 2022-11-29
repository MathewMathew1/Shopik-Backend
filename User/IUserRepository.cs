using Shop.Api.Entities;
using Shop.Api.Enums;

namespace Shop.Api.Repositories {
    public interface IUserRepository{
        Task<AuthenticateData> Login(UserLogin userLogin);
        Task<Exception> SignUp(UserModel userModel);
        Task<UserModel> GetUserById(Guid id);
        Task<Double> ChangeUserRole(Guid userId, RoleEnum newRole, RoleEnum roleOfChangingRoleUser);
    }
}