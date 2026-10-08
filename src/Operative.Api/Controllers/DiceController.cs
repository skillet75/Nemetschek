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
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DiceRollResponse>>>> GetHistoryAsync(
        [FromQuery(Name = "all")] bool? all,
        [FromQuery] int? year,
        [FromQuery] string? monthYear,
        [FromQuery] string? day,
        CancellationToken cancellationToken)
    {
        var filter = DiceHistoryFilter.FromQuery(all, year, monthYear, day);
        var response = await _diceRollService.GetHistoryAsync(_currentUser.UserId, filter, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<DiceRollResponse>>.Ok(response, "Dice roll history retrieved successfully."));
    }
}
