using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Event
{
    /// <summary>
    /// Abstract class representing an event bus that can subscribe, unsubscribe, and publish events.
    /// </summary>
    /// <typeparam name="TEventBus">The type of the event bus.</typeparam>
    /// <example>
    /// <code>
    /// [Preserve] // Add Preserve to prevent the compiler from stripping the class.
    /// public class PlayerEventBus : EventBus&lt;PlayerEventBus&gt;
    /// {
    ///     Left empty. This class is only used for making EventBus&lt;PlayerEventBus&gt; exist.
    /// }
    /// </code>
    /// </example>
    public abstract class EventBus<TEventBus> : IEventBus 
        where TEventBus : EventBus<TEventBus>
    {
        #region Fields

        /// <summary>
        /// Dictionary to store event handlers by event type and handler ID.
        /// </summary>
        /// <remarks>
        /// Action is a method that receive one param typed SpecificEvent : IEvent&lt;SpecificEventBus&gt;.
        /// </remarks>
        protected readonly Dictionary<Type, Dictionary<int, Action<IEvent<TEventBus>>>> handlers = new();

        /// <summary>
        /// Lock for unsubscribe handler.
        /// </summary>
        protected readonly object _lock = new();

        /// <summary>
        /// The next handler ID to be assigned.
        /// Increase only.
        /// </summary>
        protected int nextHandlerId = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Subscribes a handler to an event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">The handler to be invoked when the event is published.</param>
        /// <returns>The ID of the subscribed handler.</returns>
        /// <remarks>
        /// Never discard return value.
        /// Caution to multiple subscribe.
        /// </remarks>
        public virtual int Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent<TEventBus>
        {
            if (handler == null) return -1;

            Type type = typeof(TEvent);
            lock (_lock)
            {
                if (!handlers.ContainsKey(type))
                {
                    handlers[type] = new Dictionary<int, Action<IEvent<TEventBus>>>();
                }

                var handlerId = nextHandlerId++;

                // A wrapper that receive IEvent<TEventBus> type arg, and cast it to TEvent type
                // then send to handler(the real event handle func).
                handlers[type][handlerId] = e => handler((TEvent)e);

                return handlerId;
            }
        }

        /// <summary>
        /// Unsubscribes a handler from an event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handlerId">The ID of the handler to be unsubscribed.</param>
        public virtual void Unsubscribe<TEvent>(int handlerId) where TEvent : IEvent<TEventBus>
        {
            lock (_lock)
            {
                Type type = typeof(TEvent);

                if (handlers.TryGetValue(type, out Dictionary<int, Action<IEvent<TEventBus>>> eventHandlers))
                {
                    eventHandlers.Remove(handlerId);

                    if (eventHandlers.Count == 0)
                    {
                        handlers.Remove(type);
                    }
                }
            }
        }

        /// <summary>
        /// Unsubscribes all event handlers.
        /// </summary>
        public virtual void UnsubscribeAll()
        {
            lock (_lock)
            {
                handlers.Clear();
            }
        }

        /// <summary>
        /// Publishes an event by invoking all handlers for the event type.
        /// It will run on a snapshot of current handlers list.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">The event to publish.</param>
        /// tips: @ will allow us to use preserved name like class/event
        public virtual void Publish<TEvent>(IEvent<TEventBus> @event) where TEvent : IEvent<TEventBus>
        {
            List<Action<IEvent<TEventBus>>> snapshot = null;

            lock (_lock)
            {
                Type type = typeof(TEvent);
                if (handlers.TryGetValue(type, out Dictionary<int, Action<IEvent<TEventBus>>> eventHandlers))
                {
                    if (eventHandlers.Count > 0)
                    {
                        // ToList() seems be faster:
                        // https://stackoverflow.com/questions/17571621/copying-a-list-to-a-new-list-more-efficient-best-practice
                        snapshot = eventHandlers.Values.ToList();
                    }
                }
            }

            if (snapshot == null) return;

            foreach (var action in snapshot)
            {
                try
                {
                    action?.Invoke(@event);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        #endregion
    }
}