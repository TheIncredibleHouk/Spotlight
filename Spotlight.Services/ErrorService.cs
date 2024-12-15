using Spotlight.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Spotlight.Services
{
    public class ErrorService : IErrorService
    {
        public ErrorService()
        {
            _logs = new List<string>();
        }

        private List<string> _logs;

        public void LogError(string message)
        {
            _logs.Add(message);
        }

        public void LogError(Exception e, string message = "")
        {
            _logs.Add("Exception caught at:" + e.StackTrace + "\nAddtional message: " + message);
        }

        public void Clear()
        {
            _logs.Clear();
        }
    }
}