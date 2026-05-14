using PrimeForce.Core.Interfaces;

namespace PrimeForce.Core.Services;

/// <summary>
/// In-memory, single-threaded pub/sub bus for the Godot main thread.
/// Uses a snapshot (ToList) on publish so handlers may subscribe/unsubscribe
/// during dispatch without causing collection-modified exceptions.
/// </summary>
public sealed class SimpleEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : notnull
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var list))
            _handlers[typeof(TEvent)] = list = new List<Delegate>();
        list.Add(handler);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : notnull
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var list))
            list.Remove(handler);
    }

    public void Publish<TEvent>(TEvent gameEvent) where TEvent : notnull
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var list)) return;
        foreach (var handler in list.ToList())
            ((Action<TEvent>)handler)(gameEvent);
    }
}
