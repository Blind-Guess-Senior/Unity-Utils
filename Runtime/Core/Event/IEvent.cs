namespace Core.Event
{
    /// <summary>
    /// Empty interface used for event queue or other things to store all type of game event.
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// Interface for events with a specific event bus.
    /// </summary>
    /// <typeparam name="TEventBus">The type of the event bus.</typeparam>
    /// <example>
    /// <code>
    /// Define an event 'PlayerJumpEvent' on the 'PlayerEventBus' bus
    ///
    /// public class PlayerJumpEvent : IEvent&lt;PlayerEventBus&gt;
    /// {
    ///     public float jumpStrength;
    /// }
    ///</code>
    /// </example>
    public interface IEvent<TEventBus> : IEvent
        where TEventBus : EventBus<TEventBus>
    {
    }
}