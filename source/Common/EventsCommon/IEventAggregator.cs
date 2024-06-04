using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsCommon
{
    public interface IEventAggregator
    {
        void Subscribe<TEvent>(Action<TEvent> action);
        void Publish<TEvent>(TEvent eventToPublish);
    }
}