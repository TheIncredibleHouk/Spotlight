using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class SpotlightEvent
    {
        public SpotlightEventType EventType { get; private set; }
        public string Identifier { get; private set; }
        public object Data { get; private set; }

        public SpotlightEvent(SpotlightEventType eventType, string identifier, object data = null)
        {
            Identifier = identifier;
            EventType = eventType;
            Data = data;
        }
    }
}
