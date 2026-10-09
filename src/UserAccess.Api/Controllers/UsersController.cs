using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using UserAccess.Api.Domain.Entities;
using UserAccess.Api.Infrastructure;
using UserAccess.Api.Infrastructure.Persistence;

namespace UserAccess.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly UserDbContext _dbContext;

    public UsersController(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Post([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var existingUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            return Problem(
                detail: "A user with this email already exists.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                type: "about:blank");
        }

        if (!ImageDataUri.TryParse(request.Image, out var image))
        {
            ModelState.AddModelError(nameof(request.Image),
                $"Image must be a valid PNG, JPEG, or WebP data URI no larger than {ImageDataUri.SizeLimitDescription} when decoded.");
            return ValidationProblem(ModelState);
        }

        var user = new User(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            normalizedEmail,
            PasswordHasher.HashPassword(request.Password),
            image?.Data,
            image?.MediaType);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            ToImageDataUri(user),
            user.CreatedAtUtc);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponse<UserResponse>.Ok(response, "User created successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            ToImageDataUri(user),
            user.CreatedAtUtc));
    }

    private static string? ToImageDataUri(User user) => user.ImageData is null || user.ImageContentType is null
        ? null
        : $"data:{user.ImageContentType};base64,{Convert.ToBase64String(user.ImageData)}";
}
