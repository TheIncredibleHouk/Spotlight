using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Services
{
    public class TextService
    {
        private readonly ErrorService _errorService;
        private readonly Project _project;
        public TextService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public List<KeyValuePair<string, string>> GetTable(string tableName)
        {

            if (_project.TextTable.ContainsKey(tableName))
            {
                return _project.TextTable[tableName].OrderBy(kv => kv.Key).ToList();
            }

            return new List<KeyValuePair<string, string>>();
        }

        public List<string> TableNames()
        {
            return _project.TextTable.Keys.ToList();
        }

        public void AddTable(string tableName)
        {
            _project.TextTable.Add(tableName.ToLower(), new List<KeyValuePair<string, string>>());
        }

        public void SetTable(string tableName, List<KeyValuePair<string, string>> kvPairList)
        {
            _project.TextTable[tableName] = kvPairList;
        }
    }
}
