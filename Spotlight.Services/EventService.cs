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
            Emit(eventType, Guid.Empty, data);
        }

        public void Emit(SpotlightEventType eventType, Guid identifier, object data = null)
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

        public void Emit(SpotlightEventType eventType, int identifier, object data = null)
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

        public Guid Subscribe(SpotlightEventType eventType, Action eventResponder)
        {
            return Subscribe<object>(eventType, Guid.Empty, obj => eventResponder());
        }

        public Guid Subscribe(SpotlightEventType eventType, Guid identifier, Action eventResponder)
        {
            return Subscribe<object>(eventType, identifier, obj => eventResponder());
        }

        public Guid Subscribe(SpotlightEventType eventType, int identifier, Action eventResponder)
        {
            return Subscribe<object>(eventType, identifier, obj => eventResponder());
        }

        public Guid Subscribe<T>(SpotlightEventType eventType, Action<T> eventResponder)
        {
            return Subscribe<T>(eventType, Guid.Empty, eventResponder);
        }

        public Guid Subscribe<T>(SpotlightEventType eventType, Guid identifier, Action<T> eventResponder)
        {
            SpotlightEventKey eventKey = new SpotlightEventKey(identifier, eventType);
            if (!_eventResponders.ContainsKey(eventKey))
            {
                _eventResponders.Add(eventKey, new List<SpotlightEventResponder>());
            }

            SpotlightEventResponder responder = new SpotlightEventResponder(eventKey, obj => eventResponder((T)obj));

            _eventResponders[eventKey].Add(responder);
            _eventRespondersById[responder.Id] = responder;
            return responder.Id;
        }

        public Guid Subscribe<T>(SpotlightEventType eventType, int identifier, Action<T> eventResponder)
        {
            SpotlightEventKey eventKey = new SpotlightEventKey(identifier, eventType);
            if (!_eventResponders.ContainsKey(eventKey))
            {
                _eventResponders.Add(eventKey, new List<SpotlightEventResponder>());
            }

            SpotlightEventResponder responder = new SpotlightEventResponder(eventKey, obj => eventResponder((T)obj));

            _eventResponders[eventKey].Add(responder);
            _eventRespondersById[responder.Id] = responder;
            return responder.Id;
        }

        public void Unsubscribe(IEnumerable<Guid> subscriptionIds)
        {
            foreach (Guid subscriptionId in subscriptionIds)
            {
                Unsubscribe(subscriptionId);
            }
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            SpotlightEventResponder responder = _eventRespondersById[subscriptionId];
            _eventResponders[responder.EventKey].Remove(responder);
            _eventRespondersById.Remove(subscriptionId);
        }
    }
}
