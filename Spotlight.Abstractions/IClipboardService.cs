using Spotlight.Models;

namespace Spotlight.Abstractions
{
    public interface IClipboardService
    {
        ClipboardItem GetClipboard();
        void SetClipboard(ClipboardItem item);
        void ClearClipboard();
    }
}
