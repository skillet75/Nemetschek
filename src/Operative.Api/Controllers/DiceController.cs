using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Operative.Api.Application.Authentication;
using Operative.Api.Application.Dice;
using Shared.Contracts;

namespace Operative.Api.Controllers;

[ApiController]
[Route("api/dice")]
[Authorize]
public sealed class DiceController : ControllerBase
{
    private readonly DiceRollService _diceRollService;
    private readonly ICurrentUser _currentUser;

    public DiceController(DiceRollService diceRollService, ICurrentUser currentUser)
    {
        _diceRollService = diceRollService;
        _currentUser = currentUser;
    }

    [HttpPost("roll")]
    [ProducesResponseType(typeof(ApiResponse<DiceRollResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<DiceRollResponse>>> RollAsync(CancellationToken cancellationToken)
    {
        var response = await _diceRollService.RollAsync(_currentUser.UserId, cancellationToken);

        return Ok(ApiResponse<DiceRollResponse>.Ok(response, "Dice roll recorded successfully."));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DiceRollResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DiceRollResponse>>>> GetHistoryAsync(
        [FromQuery, Description("Optional year filter from 1 through 9999.")] int? year,
        [FromQuery, Description("Optional month filter from 1 through 12. Requires year.")] int? month,
        [FromQuery, Description("Optional day filter from 1 through 31. Requires year and month and must form a valid date.")] int? day,
        [FromQuery, Description("Optional date/time sort direction. Accepted values: asc or desc. When sumSort is also supplied, sum is sorted first and date/time breaks ties.")] string? dateSort,
        [FromQuery, Description("Optional dice-sum sort direction. Accepted values: asc or desc. When dateSort is also supplied, sum is sorted first and date/time second.")] string? sumSort,
        CancellationToken cancellationToken)
    {
        ValidateHistoryQuery(Request.Query);
        var filter = DiceHistoryFilter.FromQuery(year, month, day);
        var sort = DiceHistorySort.FromQuery(dateSort, sumSort);
        var response = await _diceRollService.GetHistoryAsync(_currentUser.UserId, filter, sort, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<DiceRollResponse>>.Ok(response, "Dice roll history retrieved successfully."));
    }

    private static void ValidateHistoryQuery(IQueryCollection query)
    {
        var supportedParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "year", "month", "day", "dateSort", "sumSort",
        };

        foreach (var (key, values) in query)
        {
            if (!supportedParameters.Contains(key))
            {
                throw new ApiException(
                    $"Unknown history query parameter '{key}'. Supported parameters are: year, month, day, dateSort, sumSort.",
                    StatusCodes.Status400BadRequest);
            }

            if (values.Count != 1)
            {
                throw new ApiException(
                    $"History query parameter '{key}' must be supplied exactly once.",
                    StatusCodes.Status400BadRequest);
            }
        }
    }
}
