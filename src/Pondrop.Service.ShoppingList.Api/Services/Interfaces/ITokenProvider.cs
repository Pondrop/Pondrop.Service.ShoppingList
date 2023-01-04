using Pondrop.Service.ShoppingList.Api.Models;
using System.Security.Claims;

namespace Pondrop.Service.ShoppingList.Api.Services.Interfaces;

public interface ITokenProvider
{
    string AuthenticateShopper(TokenRequest request);

    ClaimsPrincipal ValidateToken(string token);

    string GetClaim(ClaimsPrincipal principal, string claimName);
}

