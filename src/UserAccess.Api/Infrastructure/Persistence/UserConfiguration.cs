using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccess.Api.Domain.Entities;

namespace UserAccess.Api.Infrastructure.Persistence;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table => table.HasCheckConstraint(
            "CK_Users_Email_Format",
            "length(\"Email\") BETWEEN 6 AND 254 " +
            "AND instr(\"Email\", '@') > 1 " +
            "AND length(\"Email\") - length(replace(\"Email\", '@', '')) = 1 " +
            "AND instr(substr(\"Email\", instr(\"Email\", '@') + 1), '.') > 1 " +
            "AND instr(\"Email\", ' ') = 0 " +
            "AND instr(\"Email\", '..') = 0 " +
            "AND \"Email\" = trim(\"Email\")"));

        builder.HasKey(user => user.Id);

        builder.Property(user => user.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(254)
            .UseCollation("NOCASE")
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("UX_Users_Email");

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.ImageData);

        builder.Property(user => user.ImageContentType)
            .HasMaxLength(32);

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();
    }
}
