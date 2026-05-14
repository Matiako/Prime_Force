namespace PrimeForce.Core.Interfaces;

/// <summary>
/// Thin pub/sub bus that decouples systems (MathEngine ↔ Combat ↔ UI)
/// without them holding direct references to each other (DIP).
/// </summary>
public interface IEventBus
{
    void Publish<TEvent>(TEvent gameEvent) where TEvent : notnull;
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : notnull;
    void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : notnull;
}
