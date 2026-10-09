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

    public async Task<(IReadOnlyList<DiceRoll> Items, int TotalCount)> GetByUserAsync(Guid userId, DiceHistoryFilter? filter, DiceHistorySort? sort, int page, int pageSize, CancellationToken cancellationToken)
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

        var totalCount = await query.CountAsync(cancellationToken);

        IOrderedQueryable<DiceRoll> orderedQuery;
        if (sort is null)
        {
            orderedQuery = query
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id);
        }
        else if (sort.SumDirection is not null)
        {
            var orderedBySum = sort.SumDirection == DiceSortDirection.Descending
                ? query.OrderByDescending(x => x.Sum)
                : query.OrderBy(x => x.Sum);

            var orderedBySumAndDate = sort.DateDirection switch
            {
                DiceSortDirection.Ascending => orderedBySum.ThenBy(x => x.CreatedAtUtc),
                DiceSortDirection.Descending => orderedBySum.ThenByDescending(x => x.CreatedAtUtc),
                _ => orderedBySum.ThenByDescending(x => x.CreatedAtUtc),
            };

            orderedQuery = orderedBySumAndDate.ThenBy(x => x.Id);
        }
        else
        {
            var orderedByDate = sort.DateDirection == DiceSortDirection.Ascending
                ? query.OrderBy(x => x.CreatedAtUtc)
                : query.OrderByDescending(x => x.CreatedAtUtc);
            orderedQuery = orderedByDate.ThenBy(x => x.Id);
        }

        var items = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
