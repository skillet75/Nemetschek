using Microsoft.AspNetCore.Http;
using Shared.Contracts;

namespace Operative.Api.Application.Dice;

public enum DiceSortDirection
{
    Ascending,
    Descending,
}

public sealed record DiceHistorySort(DiceSortDirection? DateDirection, DiceSortDirection? SumDirection)
{
    public static DiceHistorySort? FromQuery(string? dateSort, string? sumSort)
    {
        if (dateSort is null && sumSort is null)
        {
            return null;
        }

        return new DiceHistorySort(
            ParseDirection(dateSort, "dateSort"),
            ParseDirection(sumSort, "sumSort"));
    }

    private static DiceSortDirection? ParseDirection(string? value, string parameter)
    {
        if (value is null)
        {
            return null;
        }

        return value switch
        {
            "asc" => DiceSortDirection.Ascending,
            "desc" => DiceSortDirection.Descending,
            _ => throw new ApiException(
                $"Invalid history sort. {parameter} must be 'asc' or 'desc'.",
                StatusCodes.Status400BadRequest),
        };
    }
}
