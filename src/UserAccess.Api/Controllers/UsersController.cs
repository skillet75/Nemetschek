using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using UserAccess.Api.Application.Registration;

namespace UserAccess.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly UserRegistrationService _registrationService;

    public UsersController(UserRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Post([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _registrationService.RegisterAsync(request, cancellationToken);
        if (response is null)
        {
            return Problem(
                detail: "A user with this email already exists.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                type: "about:blank");
        }

        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserResponse>.Ok(response, "User created successfully."));
    }
}
