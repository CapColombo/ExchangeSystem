using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Entities;

public sealed class TickEntity
{
    public int Id { get; init; }
    public ExchangeEntity Exchange { get; init; }
    public int ExchangeId { get; init; }
    public string Source { get; init; }
    public string RawJson { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class RawTickEntityConfiguration : IEntityTypeConfiguration<TickEntity>
{
    public void Configure(EntityTypeBuilder<TickEntity> builder)
    {
        builder.ToTable("tick", "exchange_system");
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.ExchangeId).IsRequired().HasColumnName("exchange_id");
        builder.Property(x => x.Source).IsRequired().HasColumnName("source");
        builder.Property(x => x.RawJson).IsRequired().HasColumnName("raw_json");
        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at").HasDefaultValue(DateTime.UtcNow);

        builder.HasOne(e => e.Exchange);
        builder.HasIndex(x => x.ExchangeId).HasDatabaseName("idx_raw_tick_client_id");
    }
}