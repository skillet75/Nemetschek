using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ImageDataUriAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is string dataUri && ImageDataUri.TryParse(dataUri, out _))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            $"Image must be a valid PNG, JPEG, or WebP data URI no larger than {ImageDataUri.SizeLimitDescription} when decoded.",
            [validationContext.MemberName ?? "Image"]);
    }
}
