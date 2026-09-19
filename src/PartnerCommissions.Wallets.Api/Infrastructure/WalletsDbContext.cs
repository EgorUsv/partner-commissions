using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;
using PartnerCommissions.Wallets.Api.Infrastructure.Models;

namespace PartnerCommissions.Wallets.Api.Infrastructure;

public sealed class WalletsDbContext(DbContextOptions<WalletsDbContext> options) : DbContext(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallets");
            entity.HasKey(x => x.PartnerExternalId);
            entity.Property(x => x.PartnerExternalId).ValueGeneratedNever();
            entity.Property(x => x.Balance).HasColumnType("numeric(18,8)");
        });

        modelBuilder.Entity<Payout>(entity =>
        {
            entity.ToTable("payouts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
            entity.Property(x => x.Amount).HasColumnType("numeric(18,8)");
            entity.HasIndex(x => x.CommissionId).IsUnique();
            entity.HasIndex(x => new { x.PartnerExternalId, x.CreatedAt });
            entity.HasOne<Wallet>()
                .WithMany()
                .HasForeignKey(x => x.PartnerExternalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
            entity.Property(x => x.CommissionIds).HasColumnType("uuid[]");
            entity.HasIndex(x => new { x.AvailableAfter, x.Id }).HasFilter("\"ProcessedAt\" IS NULL");
        });
    }
}
