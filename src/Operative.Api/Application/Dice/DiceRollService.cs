using Operative.Api.Domain.Entities;
using Shared.Contracts;

namespace Operative.Api.Application.Dice;

public sealed class DiceRollService(IDiceRollRepository diceRollRepository)
{
    public async Task<DiceRollResponse> RollAsync(Guid userId, CancellationToken cancellationToken)
    {
        var die1 = Random.Shared.Next(1, 7);
        var die2 = Random.Shared.Next(1, 7);
        var diceRoll = new DiceRoll(userId, die1, die2);

        await diceRollRepository.AddAsync(diceRoll, cancellationToken);

        return new DiceRollResponse(
            diceRoll.Id,
            diceRoll.UserId,
            diceRoll.Die1,
            diceRoll.Die2,
            diceRoll.Sum,
            diceRoll.CreatedAtUtc);
    }

    public async Task<IReadOnlyList<DiceRollResponse>> GetHistoryAsync(Guid userId, DiceHistoryFilter? filter, DiceHistorySort? sort, CancellationToken cancellationToken)
    {
        var history = await diceRollRepository.GetByUserAsync(userId, filter, sort, cancellationToken);

        return history
            .Select(x => new DiceRollResponse(
                x.Id,
                x.UserId,
                x.Die1,
                x.Die2,
                x.Sum,
                x.CreatedAtUtc))
            .ToList();
    }
}
