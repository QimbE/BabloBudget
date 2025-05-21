using System.Security.Claims;

namespace BabloBudget.Api.Controllers;

public static class ControllerExtensions
{
    public static Guid? TryParseUserId(this ClaimsPrincipal userClaims)
    {
        var claims = userClaims.Claims;
        var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userId == null)
            return null;
        
        if (!Guid.TryParse(userId.Value, out var userIdGuid) || userIdGuid == Guid.Empty)
            return null;
        
        return userIdGuid;
    }
}