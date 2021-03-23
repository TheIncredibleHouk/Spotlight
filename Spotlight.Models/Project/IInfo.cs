using System;

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