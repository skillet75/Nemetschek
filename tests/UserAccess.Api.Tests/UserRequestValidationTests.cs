using System.ComponentModel.DataAnnotations;
using Shared.Contracts;
using Xunit;

namespace UserAccess.Api.Tests;

public sealed class UserRequestValidationTests
{
    [Fact]
    public void CreateUserRequest_RejectsWeakPasswordAndMalformedEmail()
    {
        var request = new CreateUserRequest { FirstName = "Ada", LastName = "Lovelace", Email = "invalid", Password = "weak" };
        var results = Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateUserRequest.Email)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateUserRequest.Password)));
    }

    [Fact]
    public void CreateUserRequest_AcceptsValidPayload()
    {
        var request = new CreateUserRequest { FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com", Password = "Strong123!" };

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void CreateTokenRequest_RejectsMalformedEmailAndMissingPassword()
    {
        var request = new CreateTokenRequest { Email = "not-an-email", Password = string.Empty };
        var results = Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateTokenRequest.Email)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(CreateTokenRequest.Password)));
    }

    [Fact]
    public void CreateTokenRequest_AcceptsValidPayload()
    {
        var request = new CreateTokenRequest { Email = "ada@example.com", Password = "Strong123!" };

        Assert.Empty(Validate(request));
    }

    private static List<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }
}
