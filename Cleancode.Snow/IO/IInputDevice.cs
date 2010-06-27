using System;
namespace Cleancode.Snow.IO
{
    interface IInputDevice : IDevice
    {
        ushort Data { get; }
        
    }
}
