namespace BossMod;

// event subscription is a simple disposable class that unsubscribes on dispose
public sealed class EventSubscription(Action unsubscribe) : IDisposable
{
    public void Dispose() => unsubscribe();
}

// event subscription list is a simple way to use a single member to track multiple event subscriptions
public sealed class EventSubscriptions : IDisposable
{
    private Action? _dispose;

    public EventSubscriptions(params EventSubscription[] subscriptions) => Array.ForEach(subscriptions, Add);
    public void Dispose() => _dispose?.Invoke();
    public void Add(EventSubscription a) => _dispose += a.Dispose;
}

// a replacement for standard events
// this has a few pros:
// - unsubscribing is done by disposing an object returned from Subscribe call, which makes it harder to leak,
//   since there are multiple code analysis rules checking for forgotten Dispose() calls (unlike forgotten -=)
// - it is possible to use a lambda as a callback and not leak (with events, you can't easily pass same lambda to -=)
// - it is somewhat primary constructor friendly (though in many cases you still need usual constructor, as callbacks typically reference class members)
// this also has a few cons:
// - there is an extra indirection (small class) both on producer and consumer sides (TODO: consider turning Event into struct?)
// unlike standard event, there is nothing that prevents firing an event from outside owning class (if that is desired, consider making event private and exposing Subscribe wrapper)
// there is a convenience ExecuteAndSubscribe function, useful when you need to execute the callback for initial state
// feel free to add variants with more arguments when needed
public sealed class Event
{
    private Action? _ev;

    public EventSubscription Subscribe(Action a)
    {
        _ev += a;
        return new(() => _ev -= a);
    }
    public EventSubscription ExecuteAndSubscribe(Action a)
    {
        a();
        return Subscribe(a);
    }
    public void Fire() => _ev?.Invoke();
    public bool HaveSubscribers() => _ev != null;
}

public sealed class Event<T1>
{
    private Action<T1>? _ev;

    public EventSubscription Subscribe(Action<T1> a)
    {
        _ev += a;
        return new(() => _ev -= a);
    }
    public EventSubscription ExecuteAndSubscribe(Action<T1> a, T1 a1)
    {
        a(a1);
        return Subscribe(a);
    }
    public void Fire(T1 a1) => _ev?.Invoke(a1);
    public bool HaveSubscribers() => _ev != null;
}

public sealed class Event<T1, T2>
{
    private Action<T1, T2>? _ev;

    public EventSubscription Subscribe(Action<T1, T2> a)
    {
        _ev += a;
        return new(() => _ev -= a);
    }
    public EventSubscription ExecuteAndSubscribe(Action<T1, T2> a, T1 a1, T2 a2)
    {
        a(a1, a2);
        return Subscribe(a);
    }
    public void Fire(T1 a1, T2 a2) => _ev?.Invoke(a1, a2);
    public bool HaveSubscribers() => _ev != null;
}

public sealed class Event<T1, T2, T3>
{
    private Action<T1, T2, T3>? _ev;

    public EventSubscription Subscribe(Action<T1, T2, T3> a)
    {
        _ev += a;
        return new(() => _ev -= a);
    }
    public EventSubscription ExecuteAndSubscribe(Action<T1, T2, T3> a, T1 a1, T2 a2, T3 a3)
    {
        a(a1, a2, a3);
        return Subscribe(a);
    }
    public void Fire(T1 a1, T2 a2, T3 a3) => _ev?.Invoke(a1, a2, a3);
    public bool HaveSubscribers() => _ev != null;
}
