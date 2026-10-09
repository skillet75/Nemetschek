using Operative.Api.Domain.Entities;

namespace Operative.Api.Application.Dice;

public interface IDiceRollRepository
{
    Task AddAsync(DiceRoll diceRoll, CancellationToken cancellationToken);
    Task<(IReadOnlyList<DiceRoll> Items, int TotalCount)> GetByUserAsync(Guid userId, DiceHistoryFilter? filter, DiceHistorySort? sort, int page, int pageSize, CancellationToken cancellationToken);
}
