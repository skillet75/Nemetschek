namespace Operative.Api.Domain.Entities;

public sealed class DiceRoll
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int Die1 { get; private set; }
    public int Die2 { get; private set; }
    public int Sum { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private DiceRoll()
    {
    }

    public DiceRoll(Guid userId, int die1, int die2)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A dice roll must belong to a user.", nameof(userId));
        }

        if (die1 is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(die1), "A die value must be between 1 and 6.");
        }

        if (die2 is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(die2), "A die value must be between 1 and 6.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Die1 = die1;
        Die2 = die2;
        Sum = die1 + die2;
        CreatedAtUtc = DateTime.UtcNow;
    }
}