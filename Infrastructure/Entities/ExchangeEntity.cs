using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Entities;

public sealed class ExchangeEntity
{
    public int Id { get; init; }
    public string Title { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class ExchangeEntityConfiguration : IEntityTypeConfiguration<ExchangeEntity>
{
    public void Configure(EntityTypeBuilder<ExchangeEntity> builder)
    {
        builder.ToTable("exchange", "exchange_system");
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at").HasDefaultValue(DateTime.UtcNow);

        builder.HasData(new List<ExchangeEntity>
        {
            new() { Id = 1, Title = "Binance" },
            new() { Id = 2, Title = "Bittrex" },
        });
    }
}