using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;
using PartnerCommissions.Users.Infrastructure.Models;

namespace PartnerCommissions.Users.Infrastructure;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public IQueryable<InviterTreeRow> GetInviterTree(Guid externalId, int maxDepth, string direction)
        => FromExpression(() => GetInviterTree(externalId, maxDepth, direction));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.ExternalId);
            entity.Property(x => x.ExternalId)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.InviterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InviterTreeRow>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable((string?)null);
            entity.Property(x => x.ExternalId).HasColumnName("external_id");
            entity.Property(x => x.InviterId).HasColumnName("inviter_id");
            entity.Property(x => x.Level).HasColumnName("level");
        });

        modelBuilder.HasDbFunction(typeof(UsersDbContext).GetMethod(
                nameof(GetInviterTree),
                [typeof(Guid), typeof(int), typeof(string)])!)
            .HasName("get_inviter_tree");
    }
}
