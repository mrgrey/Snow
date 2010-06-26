using System;
using System.Collections.Generic;
using System.Text;

namespace Cleancode.Snow.Memory
{
    //содержит дамп памяти.. данные о том, куда его нужно зафигачить, собственно данные, размер.. позволяет сохранить и загрузить себя в файл и из файла
    class MemoryDump
    {
        ushort[] cells;
        int cellsOffset;
    }
}
