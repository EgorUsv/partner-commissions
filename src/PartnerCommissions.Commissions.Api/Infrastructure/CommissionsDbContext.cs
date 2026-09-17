using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;
using PartnerCommissions.Commissions.Domain;
using PartnerCommissions.Commissions.Infrastructure.Models;

namespace PartnerCommissions.Commissions.Infrastructure;

public sealed class CommissionsDbContext(DbContextOptions<CommissionsDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<SchemaSetting> SchemaSettings => Set<SchemaSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
            entity.HasIndex(x => x.OperationId).IsUnique();
            entity.Property(x => x.Profit).HasColumnType("numeric(18,8)");
            entity.HasIndex(x => x.OwnerExternalId);
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.ToTable("commissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
            entity.Property(x => x.Amount).HasColumnType("numeric(18,8)");
            entity.Property(x => x.SchemaType).HasConversion<string>();
            entity.HasOne<Event>()
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.EventId, x.PartnerExternalId }).IsUnique();
            entity.HasIndex(x => new { x.AvailableAfter, x.Id }).HasFilter("\"PaidAt\" IS NULL");
        });

        modelBuilder.Entity<SchemaSetting>(entity =>
        {
            entity.ToTable("schema_settings");
            entity.HasKey(x => x.SchemaType);
            entity.Property(x => x.SchemaType).HasConversion<string>();
            entity.HasIndex(x => x.Enabled)
                .IsUnique()
                .HasFilter("\"Enabled\"");
            entity.HasData(
                new SchemaSetting { SchemaType = SchemaType.Linear, Enabled = true },
                new SchemaSetting { SchemaType = SchemaType.Fibonacci, Enabled = false });
        });
    }
}
