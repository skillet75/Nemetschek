using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<DiceRoll>> GetByUserAsync(Guid userId, DiceHistoryFilter? filter, CancellationToken cancellationToken)
    {
        var query = dbContext.DiceRolls
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (filter is not null)
        {
            if (filter.Year is not null)
            {
                query = query.Where(x => x.CreatedAtUtc.Year == filter.Year.Value);
            }

            if (filter.Month is not null)
            {
                query = query.Where(x => x.CreatedAtUtc.Month == filter.Month.Value);
            }

            if (filter.Day is not null)
            {
                query = query.Where(x => x.CreatedAtUtc.Day == filter.Day.Value);
            }
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
