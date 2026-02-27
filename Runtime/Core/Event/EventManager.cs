using System;
using System.Collections.Generic;
using Reflection;
using UnityEngine;
using Utilities.Singleton;

namespace Core.Event
{
    /// <summary>
    /// Manages event buses and provides methods to subscribe and publish events.
    /// </summary>
    /// <example>
    /// <code>
    /// // Publisher example
    /// public class Player : MonoBehaviour
    /// {
    ///     public void Jump()
    ///     {
    ///         PlayerJumpEvent jumpEvent = new PlayerJumpEvent
    ///         {
    ///             jumpStrength = 5.0f
    ///         };
    ///         EventManager.Publish&lt;PlayerEventBus, PlayerJumpEvent&gt;(jumpEvent);
    ///     }
    /// }
    ///
    /// // Subscriber example
    /// public class SoundManager : MonoBehaviour
    /// {
    ///     private int handle;
    ///
    ///     private void OnEnable()
    ///     {
    ///         handle = EventManager.Subscribe&lt;PlayerEventBus, PlayerJumpEvent&gt;(OnPlayerJump);
    ///     }
    ///
    ///     private void OnDisable()
    ///     {
    ///         EventManager.Unsubscribe&lt;PlayerEventBus, PlayerJumpEvent&gt;(handle);
    ///     }
    ///
    ///     private void OnPlayerJump(PlayerJumpEvent jumpEvent)
    ///     {
    ///         // React to jump event, e.g. play sound
    ///     }
    /// }
    ///</code>
    /// </example>
    public class EventManager : Singleton<EventManager>
    {
        #region Static Fields

        /// <summary>
        /// Dictionary to store event buses by their type.
        /// </summary>
        protected static readonly Dictionary<Type, IEventBus> EventBuses = new();

        #endregion

        #region Initialization

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize()
        {
            _ = Instance;
        }

        #endregion

        #region Constructors

        public EventManager()
        {
            RegisterEventBus();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Registers all event buses by finding and instantiating classes that derived from EventBus.
        /// </summary>
        protected static void RegisterEventBus()
        {
            // Get all classes derived from generic class EventBus<>
            List<Type> types = TypeUtils.GetTypes(typeof(EventBus<>));

            foreach (var busType in types)
            {
                IEventBus createdEventBus = (IEventBus)Activator.CreateInstance(busType);
                EventBuses.Add(busType, createdEventBus);
            }
        }

        /// <summary>
        /// Gets the event bus of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the event bus.</typeparam>
        /// <returns>The event bus of the specified type.</returns>
        protected static T GetEventBus<T>() where T : EventBus<T>
        {
            return (T)EventBuses[typeof(T)];
        }

        /// <summary>
        /// Subscribes to an event by adding a handler to the event bus.
        /// </summary>
        /// <typeparam name="TEventBus">The type of the event bus.</typeparam>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">The handler(with param of the Event) to add for the event.</param>
        /// <returns>The ID of the subscribed handler.</returns>
        public static int Subscribe<TEventBus, TEvent>(Action<TEvent> handler)
            where TEventBus : EventBus<TEventBus>
            where TEvent : IEvent<TEventBus>
        {
            return GetEventBus<TEventBus>().Subscribe<TEvent>(handler);
        }

        /// <summary>
        /// Unsubscribes from an event by removing a handler from the event bus.
        /// </summary>
        /// <typeparam name="TEventBus">The type of the event bus.</typeparam>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handlerId">The ID of the handler to remove for the event.</param>
        public static void Unsubscribe<TEventBus, TEvent>(int handlerId)
            where TEventBus : EventBus<TEventBus>
            where TEvent : IEvent<TEventBus>
        {
            GetEventBus<TEventBus>().Unsubscribe<TEvent>(handlerId);
        }

        /// <summary>
        /// Unsubscribes all events by clearing the handlers dictionary in the specified event channel.
        /// </summary>
        public static void UnsubscribeAll<TEventBus>() where TEventBus : EventBus<TEventBus>
        {
            GetEventBus<TEventBus>().UnsubscribeAll();
        }

        /// <summary>
        /// Publishes an event by invoking all handlers for the event type.
        /// </summary>
        /// <typeparam name="TEventBus">The type of the event bus.</typeparam>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event to publish.</param>
        public static void Publish<TEventBus, TEvent>(TEvent @event)
            where TEventBus : EventBus<TEventBus>
            where TEvent : IEvent<TEventBus>
        {
            GetEventBus<TEventBus>().Publish<TEvent>(@event);
        }

        #endregion
    }
}