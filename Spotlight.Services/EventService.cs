using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Spotlight.Abstractions;
using Spotlight.Models;

namespace Spotlight.Services
{
    public class EventService : IEventService
    {
        private List<SpotlightEventSubscription> _eventSubscriptions;

        public EventService()
        {
            _eventSubscriptions = new List<SpotlightEventSubscription>();
        }

        public void Emit(SpotlightEventType eventType, object data)
        {
            foreach (SpotlightEventSubscription subscription in _eventSubscriptions.Where(subscription => subscription.EventType == eventType))
            {
                subscription.Handler(data);
            }
        }

        public void Emit(SpotlightEventType eventType, string identifier, object data = null)
        {
            foreach (SpotlightEventSubscription subscription in _eventSubscriptions.Where(subscription => subscription.EventType == eventType && subscription.Identifier == identifier))
            {
                subscription.Handler(data);
            }
        }


        public Guid Subscribe(SpotlightEventType eventType, Action<object> handler)
        {
            SpotlightEventSubscription subscription = new SpotlightEventSubscription(eventType, handler);
            _eventSubscriptions.Add(subscription);

            return subscription.Id;
        }

        public Guid Subscribe(SpotlightEventType eventType, string identfier, Action<object> handler)
        {
            SpotlightEventSubscription subscription = new SpotlightEventSubscription(eventType, handler, identfier);

            return subscription.Id; ;
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            SpotlightEventSubscription subscription = _eventSubscriptions.Where(subscription => subscription.Id == subscriptionId).FirstOrDefault();
            if (subscription != null)
            {
                _eventSubscriptions.Remove(subscription);
            }
        }
    }
}
