using Newtonsoft.Json;
using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Spotlight.Services
{
    public class ProjectService
    {
        private ErrorService _errorService;
        private Project _project;

        public ProjectService(ErrorService errorService)
        {
            _errorService = errorService;
        }

        public ProjectService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public Color[] GetLegacyPalette(string fileName)
        {
            var colors = new Color[0x40];
            FileStream fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[0x03 * 0x40];
            fStream.Read(data, 0, 0x03 * 0x40);
            fStream.Close();
            for (var i = 0; i < 0x040; i++)
            {
                colors[i] = Color.FromArgb(data[i * 0x03], data[i * 0x03 + 1], data[i * 0x03 + 2]);
            }

            return colors;
        }

        public Project LoadProject(string path)
        {
            Project project = JsonConvert.DeserializeObject<Project>(File.ReadAllText(path));
            project.DirectoryPath = new FileInfo(path).DirectoryName;

            _project = project;

            return project;
        }

        public void SaveProject(Project project, string basePath)
        {
            try
            {
                File.WriteAllText(string.Format(@"{0}\{1}.json", basePath, project.Name), JsonConvert.SerializeObject(project, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }

        public void SaveProject()
        {
            try
            {
                File.WriteAllText(string.Format(@"{0}\{1}.json", _project.DirectoryPath, _project.Name), JsonConvert.SerializeObject(_project, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }
    }
}