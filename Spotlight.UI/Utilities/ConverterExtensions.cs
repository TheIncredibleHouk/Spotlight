using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spotlight
{
    public static class ConverterExtensions
    {
        public static Rectangle AsRectangle(this Int32Rect rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
