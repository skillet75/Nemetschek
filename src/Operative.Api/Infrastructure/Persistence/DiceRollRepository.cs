using Operative.Api.Application.Dice;
using Operative.Api.Domain.Entities;

namespace Operative.Api.Infrastructure.Persistence;

internal sealed class DiceRollRepository(OperativeDbContext dbContext) : IDiceRollRepository
{
    public async Task AddAsync(DiceRoll diceRoll, CancellationToken cancellationToken)
    {
        dbContext.DiceRolls.Add(diceRoll);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
