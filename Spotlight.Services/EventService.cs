using System;
using System.Collections.Generic;
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
        private Dictionary<string, Func<SpotlightEvent, object>> _eventResponders;

        public EventService()
        {
            _eventResponders = new Dictionary<string, Func<SpotlightEvent, object>>();
        }

        public void Emit(string identifier, SpotlightEventType eventType, object data = null)
        {
            throw new NotImplementedException();
        }


        public Guid Subscribe(string identifier, SpotlightEventType eventType)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            throw new NotImplementedException();
        }
    }
}
