using Microsoft.EntityFrameworkCore;
using Operative.Api.Domain.Entities;

namespace Operative.Api.Infrastructure.Persistence;

public sealed class OperativeDbContext(DbContextOptions<OperativeDbContext> options) : DbContext(options)
{
    public DbSet<DiceRoll> DiceRolls => Set<DiceRoll>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DiceRollConfiguration());
    }
}