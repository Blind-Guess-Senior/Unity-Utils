using System.Collections.Generic;

// ReSharper disable CheckNamespace

namespace Artifact.UnityUtils.Core.Event
{
    /// <summary>
    /// A data structure of continually listening one event and queued them for later usage. 
    /// </summary>
    /// <typeparam name="TEventBus">The type of event bus that given event belongs to.</typeparam>
    /// <typeparam name="TEvent">The type of event that this queue will listen to.</typeparam>
    public class EventQueue<TEventBus, TEvent>
        where TEventBus : EventBus<TEventBus>
        where TEvent : IEvent<TEventBus>
    {
        #region Fields

        /// <summary>
        /// The id of this queue's listener in EventManager.
        /// </summary>
        protected readonly int _handlerID;

        /// <summary>
        /// The queue that stores all events that have been listent.
        /// </summary>
        protected readonly Queue<TEvent> _queue = new();

        /// <summary>
        /// Lock for safe receive or emit event in queue.
        /// </summary>
        protected readonly object _lock = new();

        #endregion

        #region Properties

        /// <summary>
        /// Count of events in queue.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Constructor that will subscribe listener to given event in EventManager.
        /// </summary>
        public EventQueue()
        {
            _handlerID = EventManager.Subscribe<TEventBus, TEvent>(OnEvent);
        }

        /// <summary>
        /// Destructor that will unsubscribe listener of given event in EventManager.
        /// </summary>
        ~EventQueue()
        {
            EventManager.Unsubscribe<TEventBus, TEvent>(_handlerID);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handler that receive event and store it into queue.
        /// </summary>
        /// <param name="event">The event to receive.</param>
        public void OnEvent(TEvent @event)
        {
            if (@event == null) return;
            lock (_lock)
            {
                _queue.Enqueue(@event);
            }
        }

        /// <summary>
        /// Peek at the event at the head of the queue without emit it.
        /// </summary>
        /// <returns></returns>
        public TEvent Peek()
        {
            lock (_lock)
            {
                return _queue.Count > 0 ? _queue.Peek() : default;
            }
        }

        /// <summary>
        /// Try to emit an event from queue's head.
        /// </summary>
        /// <param name="event">The emitted event.</param>
        /// <returns>True if emit success; otherwise, false.</returns>
        public bool TryEmit(out TEvent @event)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    @event = _queue.Dequeue();
                    return true;
                }
            }

            @event = default;
            return false;
        }

        /// <summary>
        /// Clear all events in queue and return them all.
        /// </summary>
        /// <returns>The list of all events in queue.</returns>
        public List<TEvent> DrainAll()
        {
            lock (_lock)
            {
                var list = new List<TEvent>(_queue);
                _queue.Clear();
                return list;
            }
        }

        /// <summary>
        /// Clear queue.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }

        #endregion
    }
}