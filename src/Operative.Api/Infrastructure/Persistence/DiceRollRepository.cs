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

    public async Task<IReadOnlyList<DiceRoll>> GetByUserAsync(Guid userId, DiceHistoryFilter? filter, DiceHistorySort? sort, CancellationToken cancellationToken)
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

        if (sort is null)
        {
            return await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        if (sort.SumDirection is not null)
        {
            var orderedBySum = sort.SumDirection == DiceSortDirection.Descending
                ? query.OrderByDescending(x => x.Sum)
                : query.OrderBy(x => x.Sum);

            var orderedQuery = sort.DateDirection switch
            {
                DiceSortDirection.Ascending => orderedBySum.ThenBy(x => x.CreatedAtUtc),
                DiceSortDirection.Descending => orderedBySum.ThenByDescending(x => x.CreatedAtUtc),
                _ => orderedBySum.ThenByDescending(x => x.CreatedAtUtc),
            };

            return await orderedQuery
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        var orderedByDate = sort.DateDirection == DiceSortDirection.Ascending
            ? query.OrderBy(x => x.CreatedAtUtc)
            : query.OrderByDescending(x => x.CreatedAtUtc);

        return await orderedByDate
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
