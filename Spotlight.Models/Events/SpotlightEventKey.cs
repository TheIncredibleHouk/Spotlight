using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models.Events
{
    public class SpotlightEventKey
    {
        public string Identifier { get; private set; }
        public SpotlightEventType Type { get; private set; }

        public SpotlightEventKey(string identifier, SpotlightEventType type)
        {
            Identifier = identifier;
            Type = type;
        }

        public override int GetHashCode()
        {
            if(Identifier == null)
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
                return ((SpotlightEventKey)obj).Identifier == Identifier &&
                       ((SpotlightEventKey)obj).Type == Type;
            }

            return false;
        }
    }
}
