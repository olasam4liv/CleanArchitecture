using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime? ProcessedOnUtc { get; set; }
    public int Attempt { get; set; }
    public string? Error { get; set; }
}

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.OccurredOnUtc).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.Attempt).HasDefaultValue(0);
        builder.Property(x => x.ProcessedOnUtc);
        builder.Property(x => x.Error);

        builder.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
    }
}
