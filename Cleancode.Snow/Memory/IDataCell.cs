using System;
namespace Cleancode.Snow.Memory
{
    interface IDataCell
    {
        ushort Data { get; set; }
        bool this[int index] { get; set; }
        DataCellType Type { get; }
    }
}
