using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Events
{
    public class SpotlightEventKey
    {
        public Guid GuidIdentifier { get; private set; }
        public int IntIdentifier { get; private set; }
        public SpotlightEventType Type { get; private set; }

        public SpotlightEventKey(Guid identifier, SpotlightEventType type)
        {
            GuidIdentifier = identifier;
            Type = type;
        }

        public SpotlightEventKey(int identifier, SpotlightEventType type)
        {
            IntIdentifier = identifier;
            Type = type;
        }

        public override int GetHashCode()
        {

            return GuidIdentifier.GetHashCode() ^ IntIdentifier.GetHashCode() ^ Type.GetHashCode();

        }


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is SpotlightEventKey eventKey)
            {
                return eventKey.GuidIdentifier == GuidIdentifier &&
                       eventKey.IntIdentifier == IntIdentifier &&
                       eventKey.Type == Type;
            }

            return false;
        }
    }
}
