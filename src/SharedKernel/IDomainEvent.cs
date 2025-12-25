namespace SharedKernel;

/// <summary>
/// Marker interface for domain events. Identifies events raised by aggregate roots
/// that should be processed through the outbox pattern for reliable event publishing.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces - Marker interface is intentional for DDD pattern
public interface IDomainEvent;
#pragma warning restore CA1040
