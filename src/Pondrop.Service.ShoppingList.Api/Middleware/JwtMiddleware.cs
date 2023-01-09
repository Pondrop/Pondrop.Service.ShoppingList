using Microsoft.IdentityModel.Tokens;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Models.User;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Pondrop.Service.ShoppingList.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtMiddleware> _logger;
    private readonly byte[] _tokenKey;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
        _tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
    }

    public async Task Invoke(HttpContext context, IUserService userService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
            AttachUserToContext(context, userService, token);

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, IUserService userService, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_tokenKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value.ToString() ?? string.Empty;
            var email = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value.ToString() ?? string.Empty;
            var userType = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Typ)?.Value.ToString() ?? string.Empty;

            userService.SetCurrentUser(new UserModel() { Id = userId, Email = email, Type = userType });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}