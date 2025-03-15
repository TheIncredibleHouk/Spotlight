using Spotlight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotlight.UI
{
    public class OpenGameObjectEditorEvent()
    {
        public GameObject SelectedGameObject { get; set; }
        public Palette SelectedPalette { get; set; }
    }
}
