using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class SpotlightEventSubscription
    {
        public SpotlightEventType EventType { get; private set; }
        public string Identifier { get; private set; }
        public Guid Id { get; private set; }
        public Action<object> Handler { get; private set; }

        public SpotlightEventSubscription(SpotlightEventType eventType, Action<object> handler, string identifier = null)
        {
            EventType = eventType;
            Handler = handler;
            Id = Guid.NewGuid();
            Identifier = identifier;
        }
    }
}
