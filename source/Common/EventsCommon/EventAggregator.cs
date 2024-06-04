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

        public void Subscribe<TEvent>(Action<TEvent> action)
        {
            if (!_subscribers.ContainsKey(typeof(TEvent)))
            {
                _subscribers[typeof(TEvent)] = new List<object>();
            }

            _subscribers[typeof(TEvent)].Add(action);
        }

        public void Publish<TEvent>(TEvent eventToPublish)
        {
            if (_subscribers.ContainsKey(typeof(TEvent)))
            {
                foreach (var subscriber in _subscribers[typeof(TEvent)])
                {
                    ((Action<TEvent>)subscriber)(eventToPublish);
                }
            }
        }
    }
}
