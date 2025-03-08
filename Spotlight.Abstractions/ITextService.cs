using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.Abstractions
{
    public interface ITextService
    {
        public List<KeyValuePair<string, string>> GetTable(string tableName);
        public List<string> TableNames();
        public void AddTable(string tableName);
        public void SetTable(string tableName, List<KeyValuePair<string, string>> kvPairList);
    }
}
