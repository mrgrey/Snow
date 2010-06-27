using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cleancode.Snow.IO
{
    interface IDevice
    {
        bool Ready { get; }
    }
}
