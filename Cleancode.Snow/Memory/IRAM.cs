using System;
namespace Cleancode.Snow.Memory
{
    interface IRAM
    {
        int CellsCount { get; }
        MemoryDump Dump(ushort startAddr, ushort endAddr);
        void LoadDump(MemoryDump dump);
        ushort this[int index] { get; set; }
    }
}
