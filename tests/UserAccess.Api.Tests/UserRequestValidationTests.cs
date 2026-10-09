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
        var request = new CreateUserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Password = "Strong123!",
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/xioAAAAASUVORK5CYII="
        };

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("data:image/png;base64,abc123")]
    [InlineData("data:image/gif;base64,R0lGODlhAQABAIAAAAUEBA==")]
    [InlineData("data:image/jpeg;base64,iVBORw0KGgoAAAANSUhEUg==")]
    public void CreateUserRequest_RejectsMalformedOrUnsupportedImage(string image)
    {
        var request = new CreateUserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Password = "Strong123!",
            Image = image
        };

        Assert.Contains(Validate(request), result => result.MemberNames.Contains(nameof(CreateUserRequest.Image)));
    }

    [Fact]
    public void CreateUserRequest_RejectsImageLargerThanFiveMiB()
    {
        var oversizedImageBytes = new byte[ImageDataUri.MaxDecodedBytes + 1];
        oversizedImageBytes[0] = 0x89;
        oversizedImageBytes[1] = 0x50;
        oversizedImageBytes[2] = 0x4E;
        oversizedImageBytes[3] = 0x47;
        oversizedImageBytes[4] = 0x0D;
        oversizedImageBytes[5] = 0x0A;
        oversizedImageBytes[6] = 0x1A;
        oversizedImageBytes[7] = 0x0A;
        var request = new CreateUserRequest
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Password = "Strong123!",
            Image = $"data:image/png;base64,{Convert.ToBase64String(oversizedImageBytes)}"
        };

        Assert.Contains(Validate(request), result => result.MemberNames.Contains(nameof(CreateUserRequest.Image)));
    }

    [Fact]
    public void ImageDataUri_TryParse_ReturnsDecodedImageAndContentType()
    {
        const string dataUri = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/xioAAAAASUVORK5CYII=";

        var isValid = ImageDataUri.TryParse(dataUri, out var image);

        Assert.True(isValid);
        Assert.NotNull(image);
        Assert.Equal("image/png", image.MediaType);
        Assert.Equal(dataUri[(dataUri.IndexOf(',') + 1)..], Convert.ToBase64String(image.Data));
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
