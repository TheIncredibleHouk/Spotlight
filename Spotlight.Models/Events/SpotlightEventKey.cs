using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Events
{
    public class SpotlightEventKey
    {
        public Guid Identifier { get; private set; }
        public SpotlightEventType Type { get; private set; }

        public SpotlightEventKey(Guid identifier, SpotlightEventType type)
        {
            Identifier = identifier;
            Type = type;
        }

        public override int GetHashCode()
        {
            if(Identifier == Guid.Empty)
            {
                return Type.GetHashCode();
            }

            return Identifier.GetHashCode() ^ Type.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is SpotlightEventKey)
            {
                SpotlightEventKey eventKey = (SpotlightEventKey)obj;

                return ((SpotlightEventKey)obj).Identifier == Identifier &&
                       ((SpotlightEventKey)obj).Type == Type;
            }

            return false;
        }
    }
}
