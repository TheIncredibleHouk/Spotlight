using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IEventService
    {
        Guid Subscribe(string identifier, SpotlightEventType eventType);
        void Emit(string identifier, SpotlightEventType eventType, object data = null);
        void Unsubscribe(Guid subscriptionId);
    }
}
