using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public class BitList
    {
        private List<byte> bitList;

        public BitList()
        {
            bitList = new List<byte>();
        }

        public void Add(byte value)
        {
            bitList.Add(value);
        }

        public int Count { get => bitList.Count; }

        public int this[int index]
        {
            get
            {
                if(index >= bitList.Count)
                {
                    return 0;
                }

                return bitList[index];
            }
        }
    }
}
