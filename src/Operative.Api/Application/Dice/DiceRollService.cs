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
}
