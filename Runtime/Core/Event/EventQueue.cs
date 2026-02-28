using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using Utilities.Singleton;

namespace Core.Event
{
    /// <summary>
    /// Type enum of queued event.
    /// </summary>
    public enum QueueEventType
    {
        /// <summary>
        /// Base on Update.
        /// </summary>
        Tick,

        /// <summary>
        /// Base on FixedUpdate.
        /// </summary>
        /// <remarks>
        /// Will be affected when timescale.
        /// </remarks>
        FixedTick,

        /// <summary>
        /// Base on Update deltaTime which will be affected when timescale.
        /// </summary>
        ScaledRenderTime,

        /// <summary>
        /// Base on Update unscaledDeltaTime which won't be affected when timescale.
        /// </summary>
        RenderTime,

        /// <summary>
        /// Base on FixedUpdate unscaledDeltaTime which will be affected when timescale.
        /// </summary>
        ScaledFixedTime,

        /// <summary>
        /// Base on FixedUpdate deltaTime which is definitely the same as real time.
        /// tips: fixedDeltaTime won't change when timescale.
        /// </summary>
        FixedTime,
    }

    /// <summary>
    /// Event queue for given eventbus.
    /// </summary>
    public class EventQueue : MonoPersistentSingleton<EventQueue>
    {
        #region Fields

        /// <summary>
        /// Time tick counter for render ticked queued event.
        /// </summary>
        private ulong _tick;

        /// <summary>
        /// Time tick counter for fixed ticked queued event.
        /// </summary>
        private ulong _fixedTick;

        /// <summary>
        /// Time second counter for scalable render timed queued event.
        /// </summary>
        private float _scaledRenderTime;

        /// <summary>
        /// Time second counter for render timed queued event.
        /// </summary>
        private float _renderTime;

        /// <summary>
        /// Time second counter for scalable fixed timed queued event.
        /// </summary>
        private float _scaledFixedTime;

        /// <summary>
        /// Time second counter for fixed timed queued event.
        /// </summary>
        private float _fixedTime;

        /// <summary>
        /// Event id which will be allocated to next event.
        /// Unique for every event and every different time.
        /// </summary>
        private int _nextEventId;

        /// <summary>
        /// Lock for safe edit queue.
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// Queued event struct. Store event type and its time.
        /// </summary>
        private struct QueueEvent
        {
            public IEvent Event;
            public Type EventBusType;
            public Type EventType;
            public QueueEventType TimerType;
            public ulong Tick;
            public float Time;
        }

        /// <summary>
        /// Event queue.
        /// </summary>
        private readonly Dictionary<int, QueueEvent> _queue = new();

        /// <summary>
        /// Reusable list to store events that should be published in this frame.
        /// </summary>
        private readonly List<QueueEvent> _toPublishUpdate = new();

        private readonly List<QueueEvent> _toPublishFixedUpdate = new();

        /// <summary>
        /// Cache event bus type for event type to reduce reflection usage. 
        /// </summary>
        private readonly Dictionary<Type, Type> _cachedEventBusType = new();

        /// <summary>
        /// Cache EventManager.Publish method to improve performance of reflection.
        /// </summary>
        private readonly MethodInfo _publishGenericMethod =
            typeof(EventManager).GetMethod(nameof(EventManager.Publish));

        /// <summary>
        /// Cache publish methods for given event type.
        /// It would warp method into a delegate to reduces reflection call cost.
        /// </summary>
        private readonly Dictionary<Type, Action<IEvent>> _cachedPublishDelegates = new();

        /// <summary>
        /// Lock for safe edit cached publish methods.
        /// </summary>
        private readonly object _cachedPublishDelegatesLock = new();

        /// <summary>
        /// Buckets of all tick-based event.
        /// Events which are nearest to current time will be on the front.
        /// tips: Queue used for situations that some events has the same time. 
        /// </summary>
        private readonly Dictionary<QueueEventType, SortedDictionary<ulong, Queue<int>>> _tickBuckets =
            new()
            {
                { QueueEventType.Tick, new SortedDictionary<ulong, Queue<int>>() },
                { QueueEventType.FixedTick, new SortedDictionary<ulong, Queue<int>>() }
            };

        /// <summary>
        /// Buckets of all time-based event.
        /// Events which are nearest to current time will be on the front.
        /// tips: Queue used for situations that some events has the same time. 
        /// </summary>
        private readonly Dictionary<QueueEventType, SortedDictionary<float, Queue<int>>> _timeBuckets =
            new()
            {
                { QueueEventType.ScaledRenderTime, new SortedDictionary<float, Queue<int>>() },
                { QueueEventType.RenderTime, new SortedDictionary<float, Queue<int>>() },
                { QueueEventType.ScaledFixedTime, new SortedDictionary<float, Queue<int>>() },
                { QueueEventType.FixedTime, new SortedDictionary<float, Queue<int>>() }
            };

        #endregion

        #region Unity Event Methods

        /// <summary>
        /// Check if publish method is found.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occur when cannot find EventManager.Publish method.</exception>
        private void Start()
        {
            if (_publishGenericMethod == null)
            {
                throw new InvalidOperationException("Cannot find EventManager.Publish method");
            }
        }

        /// <summary>
        /// Update method to increase relative counter and trigger publish when it meets the conditions.
        /// </summary>
        private void Update()
        {
            _tick++;
            _scaledRenderTime += Time.deltaTime;
            _renderTime += Time.unscaledDeltaTime;

            _toPublishUpdate.Clear();

            ProcessTickBucket(QueueEventType.Tick, _tick, _toPublishUpdate);

            ProcessTimeBucket(QueueEventType.ScaledRenderTime, _scaledRenderTime, _toPublishUpdate);
            ProcessTimeBucket(QueueEventType.RenderTime, _renderTime, _toPublishUpdate);

            foreach (var e in _toPublishUpdate)
            {
                Publish(e);
            }
        }

        /// <summary>
        /// FixedUpdate method to increase relative counter and trigger publish when it meets the conditions.
        /// </summary>
        private void FixedUpdate()
        {
            _fixedTick++;
            _scaledFixedTime += Time.fixedDeltaTime;
            _fixedTime += Time.fixedUnscaledDeltaTime;

            _toPublishFixedUpdate.Clear();

            ProcessTickBucket(QueueEventType.FixedTick, _fixedTick, _toPublishFixedUpdate);

            ProcessTimeBucket(QueueEventType.ScaledFixedTime, _scaledFixedTime, _toPublishFixedUpdate);
            ProcessTimeBucket(QueueEventType.FixedTime, _fixedTime, _toPublishFixedUpdate);

            foreach (var e in _toPublishFixedUpdate)
            {
                Publish(e);
            }
        }

        #endregion

        #region Queue & Publish Methods

        /// <summary>
        /// Add an event into queue. It will be published when meet the time conditions.
        /// Only used for Tick based event.
        /// </summary>
        /// <param name="event">The event want to be enqueued.</param>
        /// <param name="queueEventType">The type of queued event.</param>
        /// <param name="tick">The value for waiting. Meaning changes for different QueueEventType.</param>
        /// <returns>The event id in queue.</returns>
        /// <exception cref="ArgumentException">Occur when <see cref="QueueEventType"/> wrong.</exception>
        public int Enqueue(IEvent @event, QueueEventType queueEventType, ulong tick)
        {
            int eventId;
            var eventType = @event.GetType();
            var eventBusType = GetEventBusTypeCached(eventType) ??
                               throw new ArgumentException("Event does not implement IEvent<TEventBus>");
            var queueEvent = new QueueEvent
            {
                Event = @event,
                EventType = eventType,
                EventBusType = eventBusType,
                TimerType = queueEventType,
            };
            _ = queueEventType switch
            {
                QueueEventType.Tick => queueEvent.Tick = _tick + tick,
                QueueEventType.FixedTick => queueEvent.Tick = _fixedTick + tick,
                _ => throw new ArgumentException("Wrong queue event type"),
            };

            lock (_lock)
            {
                eventId = _nextEventId++;
                _queue.Add(eventId, queueEvent);
                AddToTickBucket(queueEventType, queueEvent.Tick, eventId);
            }

            return eventId;
        }

        /// <summary>
        /// Add an event into queue. It will be published when meet the time conditions.
        /// Only used for Time based event.
        /// </summary>
        /// <param name="event">The event want to be enqueued.</param>
        /// <param name="queueEventType">The type of queued event.</param>
        /// <param name="time">The value for waiting. Meaning changes for different QueueEventType.</param>
        /// <returns>The event id in queue.</returns>
        /// <exception cref="ArgumentException">Occur when <see cref="QueueEventType"/> wrong.</exception>
        /// <remarks>
        /// Time unit: second
        /// </remarks>
        public int Enqueue(IEvent @event, QueueEventType queueEventType, float time)
        {
            int eventId;
            var eventType = @event.GetType();
            var eventBusType = GetEventBusTypeCached(eventType) ??
                               throw new ArgumentException("Event does not implement IEvent<TEventBus>");
            var queueEvent = new QueueEvent
            {
                Event = @event,
                EventType = eventType,
                EventBusType = eventBusType,
                TimerType = queueEventType,
            };
            _ = queueEventType switch
            {
                QueueEventType.ScaledRenderTime => queueEvent.Time = _scaledRenderTime + time,
                QueueEventType.RenderTime => queueEvent.Time = _renderTime + time,
                QueueEventType.ScaledFixedTime => queueEvent.Time = _scaledFixedTime + time,
                QueueEventType.FixedTime => queueEvent.Time = _fixedTime + time,
                _ => throw new ArgumentException("Wrong queue event type"),
            };

            lock (_lock)
            {
                eventId = _nextEventId++;
                _queue.Add(eventId, queueEvent);
                AddToTimeBucket(queueEventType, queueEvent.Time, eventId);
            }

            return eventId;
        }

        /// <summary>
        /// Overload method for %int% type tick param.
        /// </summary>
        /// <param name="event">The event want to be enqueued.</param>
        /// <param name="queueEventType">The type of queued event.</param>
        /// <param name="tick">The value for waiting. Meaning changes for different QueueEventType.</param>
        /// <returns>The event id in queue.</returns>
        public int Enqueue(IEvent @event, QueueEventType queueEventType, int tick)
            => Enqueue(@event, queueEventType, (ulong)tick);

        /// <summary>
        /// Overload method with default QueueEventType FixedTick.
        /// </summary>
        /// <param name="event">The event want to be enqueued.</param>
        /// <param name="tick">The value of ticks for waiting.</param>
        /// <returns>The event id in queue.</returns>
        public int Enqueue(IEvent @event, ulong tick)
            => Enqueue(@event, QueueEventType.FixedTick, tick);

        public int Enqueue(IEvent @event, int tick)
            => Enqueue(@event, QueueEventType.FixedTick, (ulong)tick);

        /// <summary>
        /// Overload method for %double% type time param.
        /// </summary>
        /// <param name="event">The event want to be enqueued.</param>
        /// <param name="queueEventType">The type of queued event.</param>
        /// <param name="time">The value for waiting. Meaning changes for different QueueEventType.</param>
        /// <returns>The event id in queue.</returns>
        public int Enqueue(IEvent @event, QueueEventType queueEventType, double time)
            => Enqueue(@event, queueEventType, (float)time);

        /// <summary>
        /// Overload method with default QueueEventType ScaledFixedTime.
        /// </summary>
        /// <param name="event">The event want to be enqueued.</param>
        /// <param name="time">The value of seconds for waiting.</param>
        /// <returns>The event id in queue.</returns>
        public int Enqueue(IEvent @event, float time)
            => Enqueue(@event, QueueEventType.ScaledFixedTime, time);

        public int Enqueue(IEvent @event, double time)
            => Enqueue(@event, QueueEventType.ScaledFixedTime, (float)time);

        /// <summary>
        /// Remove an event from event queue by event id.
        /// </summary>
        /// <param name="eventId">The event id given by Enqueue.</param>
        /// <returns>True if remove successful; otherwise, false.</returns>
        public bool Dequeue(int eventId)
        {
            lock (_lock)
            {
                return _queue.Remove(eventId);
            }
        }

        /// <summary>
        /// Publish given event to eventbus.
        /// </summary>
        /// <param name="event">The event want to be published.</param>
        private void Publish(QueueEvent @event)
        {
            var eventType = @event.EventType;

            if (_cachedPublishDelegates.TryGetValue(eventType, out var action))
            {
                action(@event.Event);
                return;
            }

            var busType = @event.EventBusType;
            var publishMethod = _publishGenericMethod.MakeGenericMethod(busType, eventType);

            // Wrap MethodInfo into a delegate.
            // It looks like
            // Publish<%busType, %eventType>((%eventType)e)
            // for given e = @event passed in.
            var param = Expression.Parameter(typeof(IEvent), "e");
            var castParam = Expression.Convert(param, eventType);
            var call = Expression.Call(null, publishMethod, castParam);
            var lambda = Expression.Lambda<Action<IEvent>>(call, param);
            var compiled = lambda.Compile();

            Action<IEvent> actionDelegate;
            lock (_cachedPublishDelegatesLock)
            {
                if (!_cachedPublishDelegates.TryGetValue(eventType, out actionDelegate))
                {
                    _cachedPublishDelegates[eventType] = compiled;
                    actionDelegate = compiled;
                }
            }

            actionDelegate(@event.Event);
        }

        #endregion

        #region Buckets Methods

        /// <summary>
        /// Add an event into tick-based bucket.
        /// </summary>
        /// <param name="type">The type of queued event. Used to select bucket.</param>
        /// <param name="tick">The tick value of given event.</param>
        /// <param name="eventId">The event id refers to event which will be in bucket.</param>
        private void AddToTickBucket(QueueEventType type, ulong tick, int eventId)
        {
            if (!_tickBuckets.TryGetValue(type, out var bucket))
            {
                return;
            }

            if (!bucket.TryGetValue(tick, out var q))
            {
                q = new Queue<int>();
                bucket[tick] = q;
            }

            q.Enqueue(eventId);
        }

        /// <summary>
        /// Add an event into time-based bucket.
        /// </summary>
        /// <param name="type">The type of queued event. Used to select bucket.</param>
        /// <param name="time">The time value of given event.</param>
        /// <param name="eventId">The event id refers to event which will be in bucket.</param>
        private void AddToTimeBucket(QueueEventType type, float time, int eventId)
        {
            if (!_timeBuckets.TryGetValue(type, out var bucket))
            {
                return;
            }

            var key = QuantizeTime(time);
            if (!bucket.TryGetValue(key, out var q))
            {
                q = new Queue<int>();
                bucket[key] = q;
            }

            q.Enqueue(eventId);
        }

        /// <summary>
        /// Process tick-based bucket to check all event of given type that if it has meet publish conditions. 
        /// </summary>
        /// <param name="type">The type of event want to be checked.</param>
        /// <param name="currentTick">The value of current tick to judge conditions.</param>
        /// <param name="outList">The ref of external list that will contain the events which meet conditions.</param>
        private void ProcessTickBucket(QueueEventType type, ulong currentTick, List<QueueEvent> outList)
        {
            if (!_tickBuckets.TryGetValue(type, out var bucket))
            {
                return;
            }

            lock (_lock)
            {
                while (bucket.Count > 0)
                {
                    ulong firstKey;
                    using (var e = bucket.Keys.GetEnumerator())
                    {
                        if (!e.MoveNext()) break;
                        firstKey = e.Current;
                    }

                    if (firstKey > currentTick) break;

                    var idQueue = bucket[firstKey];
                    bucket.Remove(firstKey);

                    while (idQueue.Count > 0)
                    {
                        var id = idQueue.Dequeue();
                        // Remove from bucket and add to to-publish list
                        if (_queue.Remove(id, out var qe))
                        {
                            outList.Add(qe);
                        }
                        // else this event is dequeued or unexist, skip.
                    }
                }
            }
        }

        /// <summary>
        /// Process time-based bucket to check all event of given type that if it has meet publish conditions. 
        /// </summary>
        /// <param name="type">The type of event want to be checked.</param>
        /// <param name="currentTime">The value of current time to judge conditions.</param>
        /// <param name="outList">The ref of external list that will contain the events which meet conditions.</param>
        private void ProcessTimeBucket(QueueEventType type, float currentTime, List<QueueEvent> outList)
        {
            if (!_timeBuckets.TryGetValue(type, out var bucket))
            {
                return;
            }

            float quantizedCurrentTime = QuantizeTime(currentTime);

            lock (_lock)
            {
                while (bucket.Count > 0)
                {
                    float firstKey;
                    using (var e = bucket.Keys.GetEnumerator())
                    {
                        if (!e.MoveNext()) break;
                        firstKey = e.Current;
                    }

                    if (firstKey > quantizedCurrentTime) break;

                    var idQueue = bucket[firstKey];
                    bucket.Remove(firstKey);

                    while (idQueue.Count > 0)
                    {
                        var id = idQueue.Dequeue();
                        // Remove from bucket and add to to-publish list
                        if (_queue.Remove(id, out var qe))
                        {
                            outList.Add(qe);
                        }
                        // else this event is dequeued or unexist, skip.
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get event bus type of given event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <returns>The type of event bus of given event type. If no type found, return null.</returns>
        private static Type GetEventBusType(Type eventType)
        {
            foreach (var @interface in eventType.GetInterfaces())
            {
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEvent<>))
                {
                    return @interface.GetGenericArguments()[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Get event bus type of given event type. Search cache first and would update cache.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <returns>The type of event bus of given event type. If no type found, return null.</returns>
        private Type GetEventBusTypeCached(Type eventType)
        {
            if (_cachedEventBusType.TryGetValue(eventType, out var cached))
            {
                return cached;
            }

            var found = GetEventBusType(eventType);
            _cachedEventBusType[eventType] = found;

            return found;
        }

        /// <summary>
        /// Quantize a time second value to a round value.
        /// Used to reduced minor diff cause by float precise.
        /// </summary>
        /// <param name="t">The value want to be quantized.</param>
        /// <returns>The value after quantize.</returns>
        private static float QuantizeTime(float t)
        {
            return (float)Math.Round(t * 1000f) / 1000f;
        }

        #endregion
    }
}