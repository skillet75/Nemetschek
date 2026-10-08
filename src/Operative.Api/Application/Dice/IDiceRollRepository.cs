using Operative.Api.Domain.Entities;

namespace Operative.Api.Application.Dice;

public interface IDiceRollRepository
{
    Task AddAsync(DiceRoll diceRoll, CancellationToken cancellationToken);
}
