using Shared.Contracts;

namespace Operative.Api.Application.Dice;

public sealed record DiceHistoryFilter(int? Year, int? Month, int? Day)
{
    // TODO: idk if I like the name but can't think of a better one for now
    public static DiceHistoryFilter FromQuery(bool? all, int? year, string? monthYear, string? day)
    {
        if (all == true)
        {
            return new DiceHistoryFilter(null, null, null);
        }

        if (year is null && string.IsNullOrWhiteSpace(monthYear) && string.IsNullOrWhiteSpace(day))
        {
            return new DiceHistoryFilter(null, null, null);
        }

        int? parsedYear = year;
        int? parsedMonth = null;
        int? parsedDay = null;

        if (!string.IsNullOrWhiteSpace(monthYear))
        {
            if (!TryParseMonthYear(monthYear, out var month, out var monthYearValue))
            {
                throw new ApiException("The monthYear filter must use a valid month/year format.", StatusCodes.Status400BadRequest);
            }

            parsedMonth = month;
            parsedYear ??= monthYearValue;
        }

        if (!string.IsNullOrWhiteSpace(day))
        {
            if (TryParseDay(day, out var dayValue, out var dayYear, out var dayMonth))
            {
                parsedDay = dayValue;
                parsedYear ??= dayYear;
                parsedMonth ??= dayMonth;
            }
            else if (int.TryParse(day, out var numericDay) && numericDay is >= 1 and <= 31)
            {
                parsedDay = numericDay;
            }
            else
            {
                throw new ApiException("The day filter must be a valid day or date.", StatusCodes.Status400BadRequest);
            }
        }

        if (parsedYear is not null && (parsedYear.Value < 1 || parsedYear.Value > 9999))
        {
            throw new ApiException("The year filter must be between 1 and 9999.", StatusCodes.Status400BadRequest);
        }

        if (parsedMonth is not null && (parsedMonth.Value < 1 || parsedMonth.Value > 12))
        {
            throw new ApiException("The month filter must be between 1 and 12.", StatusCodes.Status400BadRequest);
        }

        if (parsedDay is not null && (parsedDay.Value < 1 || parsedDay.Value > 31))
        {
            throw new ApiException("The day filter must be between 1 and 31.", StatusCodes.Status400BadRequest);
        }

        return new DiceHistoryFilter(parsedYear, parsedMonth, parsedDay);
    }

    private static bool TryParseMonthYear(string monthYear, out int month, out int year)
    {
        month = 0;
        year = 0;

        string[] formats = ["MM/yyyy", "MM-yyyy", "yyyy-MM", "MM/yy", "MM/yyyy"];

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(
                monthYear.Trim(),
                format,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var result))
            {
                month = result.Month;
                year = result.Year;
                return true;
            }
        }

        return false;
    }

    private static bool TryParseDay(string day, out int dayValue, out int year, out int month)
    {
        dayValue = 0;
        year = 0;
        month = 0;

        string[] formats = ["yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "yyyy/MM/dd"];

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(
                day.Trim(),
                format,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var result))
            {
                dayValue = result.Day;
                year = result.Year;
                month = result.Month;
                return true;
            }
        }

        return false;
    }
}
