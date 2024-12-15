using System;
using System.Collections.Generic;
using System.Linq;
using Spotlight.Abstractions;
using Spotlight.Models;

namespace Spotlight.Services
{
    public class ClipboardService : IClipboardService
    {
        private ClipboardItem _clipboardItem;

        public void ClearClipboard()
        {
            _clipboardItem = null;
        }

        public ClipboardItem GetClipboard()
        {
            return _clipboardItem;
        }

        public void SetClipboard(ClipboardItem item)
        {
            _clipboardItem = item;
        }
    }
}
