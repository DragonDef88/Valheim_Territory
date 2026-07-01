using System;
using System.Collections.Generic;

namespace ClanTerritory.Events
{
    internal sealed class EventBus
    {
        private readonly Dictionary<Type, List<object>> _handlers =
            new Dictionary<Type, List<object>>();

        public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null)
                return;

            Type eventType = typeof(TEvent);

            List<object> handlers;

            if (!_handlers.TryGetValue(eventType, out handlers))
            {
                handlers = new List<object>();
                _handlers[eventType] = handlers;
            }

            if (!handlers.Contains(handler))
                handlers.Add(handler);
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : IEvent
        {
            if (eventData == null)
                return;

            Type eventType = typeof(TEvent);

            List<object> handlers;

            if (!_handlers.TryGetValue(eventType, out handlers))
                return;

            for (int i = 0; i < handlers.Count; i++)
            {
                IEventHandler<TEvent> handler = handlers[i] as IEventHandler<TEvent>;

                if (handler != null)
                    handler.Handle(eventData);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}