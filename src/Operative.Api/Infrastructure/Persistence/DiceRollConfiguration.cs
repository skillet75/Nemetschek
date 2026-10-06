using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Operative.Api.Domain.Entities;

namespace Operative.Api.Infrastructure.Persistence;

internal sealed class DiceRollConfiguration : IEntityTypeConfiguration<DiceRoll>
{
    public void Configure(EntityTypeBuilder<DiceRoll> builder)
    {
        builder.ToTable("DiceRolls", table =>
        {
            table.HasCheckConstraint("CK_DiceRolls_Die1_Range", "\"Die1\" BETWEEN 1 AND 6");
            table.HasCheckConstraint("CK_DiceRolls_Die2_Range", "\"Die2\" BETWEEN 1 AND 6");
            table.HasCheckConstraint("CK_DiceRolls_Sum", "\"Sum\" = \"Die1\" + \"Die2\"");
        });

        builder.HasKey(diceRoll => diceRoll.Id);

        builder.Property(diceRoll => diceRoll.UserId)
            .IsRequired();

        builder.HasIndex(diceRoll => new { diceRoll.UserId, diceRoll.CreatedAtUtc })
            .HasDatabaseName("IX_DiceRolls_UserId_CreatedAtUtc");

        builder.HasIndex(diceRoll => new { diceRoll.UserId, diceRoll.Sum })
            .HasDatabaseName("IX_DiceRolls_UserId_Sum");

        builder.Property(diceRoll => diceRoll.Die1).IsRequired();
        builder.Property(diceRoll => diceRoll.Die2).IsRequired();
        builder.Property(diceRoll => diceRoll.Sum).IsRequired();
        builder.Property(diceRoll => diceRoll.CreatedAtUtc).IsRequired();
    }
}