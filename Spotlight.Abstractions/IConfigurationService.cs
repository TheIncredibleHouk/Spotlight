using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IConfigurationService
    {
        void UpdateConfiguration(Configuration configuration);
        Configuration GetConfiguration();
    }
}
