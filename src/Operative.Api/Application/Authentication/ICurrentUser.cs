namespace Operative.Api.Application.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
