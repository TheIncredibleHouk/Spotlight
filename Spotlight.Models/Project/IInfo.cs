using System;
using System.Collections.Generic;

namespace Spotlight.Models
{
    public interface IInfo
    {
        Guid Id { get; set; }
        string Name { get; set; }
        InfoType InfoType { get; }
        IInfo ParentInfo { get; set; }
        List<LevelInfo> SublevelsInfo { get; set; }
        string DisplayName { get; }
    }

    public enum InfoType
    {
        World,
        Level
    }
}