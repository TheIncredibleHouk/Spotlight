using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IProjectService
    {
        bool LoadProject(string projectPath);
        bool SaveProject(string basePath = null);
        Project GetProject();
    }
}
