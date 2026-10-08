using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Operative.Api.Application.Authentication;

namespace Operative.Api.Infrastructure.Authentication;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new InvalidOperationException("No authenticated user is available for this request.");
            }

            var subject = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(subject, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("The authenticated user does not have a valid subject claim.");
            }

            return userId;
        }
    }
}
