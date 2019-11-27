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

namespace Spotlight.Services
{
    public class ProjectService
    {
        private ErrorService _errorService;

        public ProjectService(ErrorService errorService)
        {
            _errorService = errorService;
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

        public Project ConvertProject(LegacyProject legacyProject)
        {
            return new Project()
            {
                Name = legacyProject.name,
                Path = legacyProject.romfile,
                Palettes = legacyProject.paletteinfo.Select(s => new Palette()
                {
                    Id = Guid.Parse(s.guid),
                    Colors = s.data.Split(',').Select(c => Int32.Parse(c, System.Globalization.NumberStyles.HexNumber)).ToList(),
                    Name = s.name
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
                    Number = Int32.Parse(w.ordinal)
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
            try
            {
                return JsonConvert.DeserializeObject<Project>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
                return null;
            }
        }

        public void SaveProject(Project project)
        {
            try
            {
                File.WriteAllText(project.Path, JsonConvert.SerializeObject(project));
            }
            catch (Exception e)
            {
                _errorService.LogError(e);
            }
        }
    }
}
