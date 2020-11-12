using System.Drawing;

namespace Spotlight.Models
{
    public class Configuration
    {
        public Rectangle WindowLocation { get; set; }
        public bool WindowIsMaximized { get; set; }

        public string LastProjectPath { get; set; }
    }
}