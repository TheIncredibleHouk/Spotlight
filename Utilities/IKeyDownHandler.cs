using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Spotlight
{
    public interface IKeyDownHandler
    {
        void HandleKeyDown(KeyEventArgs e);
    }
}
