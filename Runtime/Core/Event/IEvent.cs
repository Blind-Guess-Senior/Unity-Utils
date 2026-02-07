namespace Core.Event
{
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
    public interface IEvent<TEventBus> where TEventBus : EventBus<TEventBus>
    {
    }
}