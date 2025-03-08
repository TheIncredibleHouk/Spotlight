using Spotlight.Abstractions;
using Spotlight.Models;
using System.Collections.Generic;
using System.Linq;

namespace Spotlight.Services
{
    public class TextService : ITextService
    {
        private readonly IErrorService _errorService;
        private readonly IProjectService _projectService;

        public TextService(IErrorService errorService, IProjectService projectService)
        {
            _errorService = errorService;
            _projectService = projectService;
        }

        public List<KeyValuePair<string, string>> GetTable(string tableName)
        {
            Project project = _projectService.GetProject();
            if (project.TextTable.ContainsKey(tableName))
            {
                return project.TextTable[tableName].ToList();
            }

            return new List<KeyValuePair<string, string>>();
        }

        public List<string> TableNames()
        {
            return _projectService.GetProject().TextTable.Keys.ToList();
        }

        public void AddTable(string tableName)
        {
            _projectService.GetProject().TextTable.Add(tableName.ToLower(), new List<KeyValuePair<string, string>>());
        }

        public void SetTable(string tableName, List<KeyValuePair<string, string>> kvPairList)
        {
            _projectService.GetProject().TextTable[tableName] = kvPairList;
        }
    }
}