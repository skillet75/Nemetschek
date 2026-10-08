using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using UserAccess.Api.Application.Authentication;

namespace UserAccess.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly TokenAuthenticationService _authenticationService;

    public AuthController(TokenAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("token")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthTokenResponse>>> PostToken(
        [FromBody] CreateTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _authenticationService.AuthenticateAsync(request, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new ErrorResponse(
                "Invalid email or password.",
                StatusCodes.Status401Unauthorized,
                HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<AuthTokenResponse>.Ok(response, "Authentication successful."));
    }
}
