using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Models.User;

namespace Pondrop.Service.ShoppingList.Api.Services;

public class UserService : IUserService
{
    public string GetMaterializedViewUserName() => "materialized_view";

    public string UserId { get; private set; }
    public string UserName { get; private set; }

    public UserType UserType { get; private set; }

    public UserService()
    {
        UserId = "admin";
        UserName = "admin";
        UserType = UserType.Shopper;
    }

    public string CurrentUserId() => UserId;
    public string CurrentUserName() => UserName;
    public UserType CurrentUserType() => UserType;

    public bool SetCurrentUser(Service.Models.User.UserModel user)
    {
        if (user is null)
            return false;

        UserId = user.Id ?? string.Empty;
        UserName = user.Email ?? string.Empty;
        UserType = Enum.TryParse(user.Type, out UserType userType) ? userType : UserType.Shopper;
        return true;
    }
}