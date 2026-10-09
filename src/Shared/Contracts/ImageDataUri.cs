namespace Shared.Contracts;

public sealed record ImageDataUri(byte[] Data, string MediaType)
{
    public const int MaxDecodedBytes = 5 * 1024 * 1024;
    public const string SizeLimitDescription = "5 MiB";

    public static bool TryParse(string? value, out ImageDataUri? image)
    {
        image = null;
        if (value is null)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var separatorIndex = value.IndexOf(',', StringComparison.Ordinal);
        if (separatorIndex < 0)
        {
            return false;
        }

        var mediaType = value[..separatorIndex] switch
        {
            "data:image/png;base64" => "image/png",
            "data:image/jpeg;base64" => "image/jpeg",
            "data:image/webp;base64" => "image/webp",
            _ => null,
        };

        if (mediaType is null)
        {
            return false;
        }

        var base64 = value[(separatorIndex + 1)..];
        var maxEncodedLength = ((MaxDecodedBytes + 2) / 3) * 4;
        if (base64.Length == 0 || base64.Length > maxEncodedLength || base64.Length % 4 != 0)
        {
            return false;
        }

        byte[] data;
        try
        {
            data = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return false;
        }

        if (data.Length > MaxDecodedBytes || !MatchesImageSignature(data, mediaType))
        {
            return false;
        }

        image = new ImageDataUri(data, mediaType);
        return true;
    }

    private static bool MatchesImageSignature(ReadOnlySpan<byte> bytes, string mediaType) => mediaType switch
    {
        "image/png" => bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
        }),
        "image/jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
        "image/webp" => bytes.Length >= 12 &&
                        bytes[..4].SequenceEqual("RIFF"u8) &&
                        bytes[8..12].SequenceEqual("WEBP"u8),
        _ => false,
    };
}
