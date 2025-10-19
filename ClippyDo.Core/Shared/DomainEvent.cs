namespace ClippyDo.Core.Shared;

public abstract record DomainEvent
{
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}