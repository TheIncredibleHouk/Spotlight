using Spotlight.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;

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

        public string RomFileName
        {
            get
            {
                return _project.RomFilePath;
            }
            set
            {
                _project.RomFilePath = value;
            }
        }

        public ProjectService(ErrorService errorService, Project project)
        {
            _errorService = errorService;
            _project = project;
        }

        public LegacyProject GetLegacyProject(string fileName)
        {
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                using (XmlReader xmlReader = XmlReader.Create(fileStream))
                {
                    try
                    {
                        return ((LegacyProject)new XmlSerializer(typeof(LegacyProject)).Deserialize(xmlReader));
                    }
                    catch (Exception e)
                    {
                        _errorService.LogError(e);
                        return null;
                    }
                }
            }
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

        public Project ConvertProject(LegacyProject legacyProject)
        {
            return new Project()
            {
                Name = legacyProject.name,
                RomFilePath = legacyProject.romfile,
                Palettes = legacyProject.paletteinfo.Select(s => new Palette()
                {
                    IndexedColors = s.data.Split(',').Select(c => Int32.Parse(c, System.Globalization.NumberStyles.HexNumber)).ToArray(),
                    Name = s.name,
                    Id = s.guid
                }).ToList(),
                WorldInfo = legacyProject.worldinfo.Where(w => w.isnoworld == "false").Select(w => new WorldInfo()
                {
                    Id = Guid.Parse(w.worldguid),
                    LastModified = DateTime.Parse(w.lastmodified),
                    Name = w.name,
                    Number = Int32.Parse(w.ordinal),
                    LevelsInfo = legacyProject.levelinfo.Where(l => l.worldguid == w.worldguid && l.bonusfor == Guid.Empty.ToString().ToLower()).Select(l => new LevelInfo()
                    {
                        Id = Guid.Parse(l.levelguid),
                        LastModified = DateTime.Parse(l.lastmodified),
                        Name = l.name,
                        TileSet = Int32.Parse(l.leveltype),
                        SublevelsInfo = GetBonusLevels(l.levelguid, legacyProject.levelinfo.Where(b => b.bonusfor != Guid.Empty.ToString().ToLower()).ToList())
                    }).ToList()
                }).ToList(),
                EmptyWorld = legacyProject.worldinfo.Where(w => w.isnoworld == "true").Select(w => new WorldInfo()
                {
                    Id = Guid.Parse(w.worldguid),
                    LastModified = DateTime.Parse(w.lastmodified),
                    Name = w.name,
                    Number = Int32.Parse(w.ordinal),
                    LevelsInfo = legacyProject.levelinfo.Where(l => l.worldguid == w.worldguid && l.bonusfor == Guid.Empty.ToString().ToLower()).Select(l => new LevelInfo()
                    {
                        Id = Guid.Parse(l.levelguid),
                        LastModified = DateTime.Parse(l.lastmodified),
                        Name = l.name,
                        TileSet = Int32.Parse(l.leveltype),
                        SublevelsInfo = GetBonusLevels(l.levelguid, legacyProject.levelinfo.Where(b => b.bonusfor != Guid.Empty.ToString().ToLower()).ToList())
                    }).ToList()
                }).First(),

            };
        }

        private List<LevelInfo> GetBonusLevels(string levelGuid, List<LegacyLevelInfo> levels)
        {
            var bonusLevels = levels.Where(l => l.bonusfor == levelGuid).ToList();
            if (bonusLevels.Count == 0)
            {
                return null;
            }

            return bonusLevels.Select(l => new LevelInfo()
            {
                Id = Guid.Parse(l.levelguid),
                LastModified = DateTime.Parse(l.lastmodified),
                Name = l.name,
                TileSet = Int32.Parse(l.leveltype),
                SublevelsInfo = GetBonusLevels(l.levelguid, levels)
            }).ToList();
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
                File.WriteAllText(string.Format(@"{0}\{1}.json", basePath, project.Name), JsonConvert.SerializeObject(project));
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
                File.WriteAllText(string.Format(@"{0}\{1}.json", _project.DirectoryPath, _project.Name), JsonConvert.SerializeObject(_project));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }
    }
}
