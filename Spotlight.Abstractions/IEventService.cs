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
        Guid Subscribe(SpotlightEventType eventType, Action function);
        Guid Subscribe(SpotlightEventType eventType, Guid identifier, Action function);
        Guid Subscribe(SpotlightEventType eventType, int identifier, Action function);
        Guid Subscribe<T>(SpotlightEventType eventType, Action<T> function);
        Guid Subscribe<T>(SpotlightEventType eventType, Guid identifier, Action<T> function);
        Guid Subscribe<T>(SpotlightEventType eventType, int identifier, Action<T> function);
        void Emit(SpotlightEventType eventType, object data = null);
        void Emit(SpotlightEventType eventType, Guid identifier, object data = null);
        void Emit(SpotlightEventType eventType, int identifier, object data = null);
        void Unsubscribe(Guid subscriptionId);
        void Unsubscribe(IEnumerable<Guid> subscriptionIds);
    }
}
