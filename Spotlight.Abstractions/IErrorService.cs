using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface IErrorService
    {
        void LogError(string message);
        void LogError(Exception exception, string message);
        void Clear();
    }
}
