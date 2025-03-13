using Newtonsoft.Json;
using Spotlight.Abstractions;
using Spotlight.Models;
using System;
using System.IO;

namespace Spotlight.Services
{
    public class ProjectService : IProjectService
    {
        private IErrorService _errorService;
        private IEventService _eventService;
        private Project _currentProject;

        public ProjectService(IErrorService errorService, IEventService eventService)
        {
            _errorService = errorService;
            _eventService = eventService;
        }

        public ProjectService()
        {
            //_errorService = errorService;
            //_project = project;
        }

        //public Color[] GetLegacyPalette(string fileName)
        //{
        //    var colors = new Color[0x40];
        //    FileStream fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        //    byte[] data = new byte[0x03 * 0x40];
        //    fStream.Read(data, 0, 0x03 * 0x40);
        //    fStream.Close();
        //    for (var i = 0; i < 0x040; i++)
        //    {
        //        colors[i] = Color.FromArgb(data[i * 0x03], data[i * 0x03 + 1], data[i * 0x03 + 2]);
        //    }

        //    return colors;
        //}

        public bool LoadProject(string path)
        {
            if (!File.Exists(path))
            {
                _errorService.LogError($"File {path} does not exist.");

                return false;
            }

            try
            {
                Project project = JsonConvert.DeserializeObject<Project>(File.ReadAllText(path));
                project.DirectoryPath = new FileInfo(path).DirectoryName;

                _currentProject = project;

                foreach (WorldInfo worldInfo in project.WorldInfo)
                {
                    LinkLevelTree(worldInfo);
                }

                LinkLevelTree(project.EmptyWorld);

                _eventService.Emit(SpotlightEventType.ProjectLoaded);

                return true;
            }
            catch (Exception ex)
            {
                _errorService.LogError(ex, $"Error occurred deserializing the project at {path}.");
                return false;
            }
        }

        public bool SaveProject(string basePath = null)
        {
            string directoryPath = basePath ?? _currentProject.DirectoryPath;

            if (!Directory.Exists(directoryPath))
            {
                _errorService.LogError($"Directory path ${directoryPath} does not exist");
                return false;
            }

            string projectFilePath = string.Format(@"{0}\{1}.json", directoryPath, _currentProject.Name);
            string fileContents = JsonConvert.SerializeObject(_currentProject, Newtonsoft.Json.Formatting.Indented);

            try
            {
                File.WriteAllText(projectFilePath, fileContents);
                _eventService.Emit(SpotlightEventType.ProjectUpdated);
                return true;
            }
            catch (Exception e)
            {
                _errorService.LogError(e, $"Error currect trying to save file contents to {projectFilePath}");
                return false;
            }
        }

        public Project GetProject() => _currentProject;

        private void LinkLevelTree(IInfo iInfo)
        {
            if (iInfo.SublevelsInfo == null)
            {
                return;
            }

            foreach (IInfo subInfo in iInfo.SublevelsInfo)
            {
                subInfo.ParentInfo = iInfo;
                LinkLevelTree(subInfo);
            }
        }
    }
}