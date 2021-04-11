using System.Windows;
using System.Windows.Media;

namespace Spotlight
{
    public static class DpiHelper
    {
        public static Dpi GetDpi(this Visual visualElement)
        {
            PresentationSource source = PresentationSource.FromVisual(visualElement);

            double dpiX = 96, dpiY = 96;
            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            return new Dpi()
            {
                X = dpiX,
                Y = dpiY
            };
        }
    }

    public class Dpi
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
