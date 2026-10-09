using Microsoft.AspNetCore.Http;
using Shared.Contracts;

namespace Operative.Api.Application.Dice;

public sealed record DiceHistoryFilter(int? Year, int? Month, int? Day)
{
    public static DiceHistoryFilter FromQuery(int? year, int? month, int? day)
    {
        if (year is < 1 or > 9999)
        {
            throw InvalidFilter("year must be between 1 and 9999.");
        }

        if (month is < 1 or > 12)
        {
            throw InvalidFilter("month must be between 1 and 12.");
        }

        if (day is < 1 or > 31)
        {
            throw InvalidFilter("day must be between 1 and 31.");
        }

        if (month is not null && year is null)
        {
            throw InvalidFilter("month requires year.");
        }

        if (day is not null && month is null)
        {
            throw InvalidFilter("day requires both year and month.");
        }

        if (day is not null && !DateOnly.TryParseExact(
                $"{year:D4}-{month:D2}-{day:D2}",
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out _))
        {
            throw InvalidFilter("year, month, and day must form a valid calendar date.");
        }

        return new DiceHistoryFilter(year, month, day);
    }

    private static ApiException InvalidFilter(string message) =>
        new($"Invalid history filter. {message}", StatusCodes.Status400BadRequest);
}
