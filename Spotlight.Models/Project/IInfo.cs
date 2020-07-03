using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Models
{
    public interface IInfo
    {
        Guid Id { get; set; }
        string Name { get; set; }
        InfoType InfoType { get; }

        string DisplayName { get; }
    }

    public enum InfoType
    {
        World,
        Level
    }
}
