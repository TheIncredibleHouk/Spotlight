using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Events
{
    public class SpotlightEventResponder
    {
        public SpotlightEventKey EventKey { get; protected set; }
        public Action<object> Responder { get; protected set; }
        public Guid Id { get; protected set; }

        public SpotlightEventResponder(SpotlightEventKey eventKey, Action<object> responder)
        {
            EventKey = eventKey;
            Responder = responder;
            Id = Guid.NewGuid();
        }
    }
}
