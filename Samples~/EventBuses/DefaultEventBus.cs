using UnityEngine.Scripting;

namespace Core.Event.EventBuses
{
    /// <summary>
    /// This is a sample of EventBus. You can just make your own event bus with the same pattern as this one.
    /// </summary>
    [Preserve]
    public class DefaultEventBus : EventBus<DefaultEventBus>
    {
    }
}