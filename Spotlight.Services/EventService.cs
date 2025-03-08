using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Spotlight.Abstractions;
using Spotlight.Models;
using Spotlight.Models.Events;

namespace Spotlight.Services
{
    public class EventService : IEventService
    {
        private Dictionary<SpotlightEventKey, List<SpotlightEventResponder>> _eventResponders;
        private Dictionary<Guid, SpotlightEventResponder> _eventRespondersById;

        public EventService()
        {
            _eventResponders = new Dictionary<SpotlightEventKey, List<SpotlightEventResponder>>();
            _eventRespondersById = new Dictionary<Guid, SpotlightEventResponder>();
        }

        public void Emit(SpotlightEventType eventType, object data = null)
        {
            Emit(eventType, null, data);
        }

        public void Emit(SpotlightEventType eventType, string identifier, object data = null)
        {
            SpotlightEventKey eventKey = new SpotlightEventKey(identifier, eventType);

            if (_eventResponders.ContainsKey(eventKey))
            {
                foreach (SpotlightEventResponder eventResponder in _eventResponders[eventKey])
                {
                    eventResponder.Responder(data);
                }
            }
        }


        public Guid Subscribe(string identifier, SpotlightEventType eventType, Action<object> eventResponder)
        {
            SpotlightEventKey eventKey = new SpotlightEventKey(identifier, eventType);
            if (!_eventResponders.ContainsKey(eventKey))
            {
                _eventResponders.Add(eventKey, new List<SpotlightEventResponder>());
            }

            SpotlightEventResponder responder = new SpotlightEventResponder(eventKey, eventResponder);

            _eventResponders[eventKey].Add(responder);
            _eventRespondersById[responder.Id] = responder;
            return responder.Id;
        }

        public Guid Subscribe(SpotlightEventType eventType, string identfier, Action<object> handler)
        {
            SpotlightEventSubscription subscription = new SpotlightEventSubscription(eventType, handler, identfier);

            return subscription.Id; ;
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            SpotlightEventResponder responder = _eventRespondersById[subscriptionId];
            _eventResponders[responder.EventKey].Remove(responder);
            _eventRespondersById.Remove(subscriptionId);
        }
    }
}
