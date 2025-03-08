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
        Guid Subscribe(SpotlightEventType eventType, Action<object> function);
        Guid Subscribe(SpotlightEventType eventType, string identifier, Action<object> function);
        void Emit(SpotlightEventType eventType, object data = null);
        void Emit(SpotlightEventType eventType, string identifier, object data = null);
        void Unsubscribe(Guid subscriptionId);
    }
}
