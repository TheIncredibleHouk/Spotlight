using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class ErrorService
    {
        public ErrorService()
        {
            CurrentLog = new List<string>();
        }

        public List<string> CurrentLog { get; private set; }

        public void LogError(string message)
        {
            CurrentLog.Add(message);
        }

        public void LogError(Exception e, string message = "")
        {
            CurrentLog.Add("Exception caught at:" + e.StackTrace + "\nAddtional message: " + message);
        }

        public void Reset()
        {
            CurrentLog.Clear();
        }
    }
}
