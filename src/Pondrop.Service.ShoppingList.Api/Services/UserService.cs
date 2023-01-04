using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Models.User;

namespace Pondrop.Service.ShoppingList.Api.Services;

public class UserService : IUserService
{
    public string CurrentUserId() => throw new NotImplementedException();
    public string CurrentUserName() => "admin";
    public UserType CurrentUserType() => throw new NotImplementedException();
    public string GetMaterializedViewUserName() => "materialized_view";
    public bool SetCurrentUser(UserModel user) => throw new NotImplementedException();
}