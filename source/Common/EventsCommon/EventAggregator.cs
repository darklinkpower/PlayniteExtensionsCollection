using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsCommon
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, List<object>> _subscribers = new Dictionary<Type, List<object>>();
        private readonly object _lock = new object();

        public void Subscribe<TEvent>(Action<TEvent> action)
        {
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(typeof(TEvent)))
                {
                    _subscribers[typeof(TEvent)] = new List<object>();
                }

                _subscribers[typeof(TEvent)].Add(action);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> action)
        {
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(typeof(TEvent)))
                {
                    return;
                }

                _subscribers[typeof(TEvent)].Remove(action);
                if (_subscribers[typeof(TEvent)].Count == 0)
                {
                    _subscribers.Remove(typeof(TEvent));
                }
            }
        }

        public void Publish<TEvent>(TEvent eventToPublish)
        {
            Action<TEvent>[] subscribersCopy;
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(typeof(TEvent)))
                {
                    return;
                }

                subscribersCopy = _subscribers[typeof(TEvent)].Cast<Action<TEvent>>().ToArray();
            }

            foreach (var subscriber in subscribersCopy)
            {
                subscriber(eventToPublish);
            }
        }

    }
}
