#region Copyright&License
//   Snow - BasePC Emulator
//   Copyright (C) 2008 cleancode.ru
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License,
//   or any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//   Authors: Denis Bykov (mailto:thorn@cleancode.ru), Chuyko Yuriy (mr_grey@cleancode.ru)
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Cleancode.Snow.Memory;
using System.IO;
using Cleancode.Snow.Microcommands;

namespace Cleancode.Snow
{
    delegate void Callback();

    [ActionsContainer()]
    class MainActionsContainer
    {
        private Callback _machineStopCallback;

        private Core.Machine _machine;

        private Dictionary<string, string> memoryTable = new Dictionary<string, string>();

        private Dictionary<string, RegisterTypes> _registerTypesMapping = new Dictionary<string, RegisterTypes>();

        private List<string> _watches;

        public uint RunLimit = 1000;

        protected MainActionsContainer() { }

        public MainActionsContainer(Core.Machine machine, Callback machineStopCallback)
        {
            if (machineStopCallback == null)
                throw new ArgumentException("cannot be null", "machineStopCallback");
            _machine = machine;
            _machineStopCallback = machineStopCallback;
            _registerTypesMapping.Add("acc", RegisterTypes.Accumulator);
            _registerTypesMapping.Add("addr", RegisterTypes.Address);
            _registerTypesMapping.Add("br", RegisterTypes.BufferRegister);
            _registerTypesMapping.Add("c", RegisterTypes.Carry);
            _registerTypesMapping.Add("cbr", RegisterTypes.CarryBufferRegister);
            _registerTypesMapping.Add("cmd", RegisterTypes.Command);
            _registerTypesMapping.Add("cc", RegisterTypes.CommandCounter);
            _registerTypesMapping.Add("data", RegisterTypes.Data);
            _registerTypesMapping.Add("kbd", RegisterTypes.Keyboard);
            _registerTypesMapping.Add("mcmd", RegisterTypes.Microcommand);
            _registerTypesMapping.Add("mcmdcounter", RegisterTypes.MircocommandsCounter);
            _registerTypesMapping.Add("state", RegisterTypes.State);

            _watches = new List<string>(new string[] { "cc" });
        }

        [Action("get runlimit")]
        public string GetRunLimit()
        {
            return RunLimit.ToString();
        }

        [Action("set runlimit")]
        public string SetRunLimit(uint runLimit)
        {
            RunLimit = runLimit;
            return "Ok";
        }

        [Action("get mem")]
        public string GetMem(ushort memAddr) 
        {
            return _printMemCells("Memory cell:", _machine.MainMemory, memAddr, memAddr);
        }

        [Action("get mem")]
        public string GetMem(ushort memAddrStart, ushort memAddrEnd)
        {
            return _printMemCells("Memory:", _machine.MainMemory, memAddrStart, memAddrEnd);
        }

        [Action("get micro")]
        public string GetMicro(ushort memAddr)
        {
            return _printMemCells("Micro cell:", _machine.MicrocommandsMemory, memAddr, memAddr);
        }

        [Action("get micro")]
        public string GetMicro(ushort memAddrStart, ushort memAddrEnd)
        {
            return _printMemCells("Micro:", _machine.MicrocommandsMemory, memAddrStart, memAddrEnd);
        }

        [Action("get reg")]
        public string GetReg(string regName)
        {
            return _printRow("Registers:", regName, _machine.Registers[_registerTypesMapping[regName]].Data.ToString("X4"));
        }

        [Action("get regs")]
        public string GetRegs()
        {
            foreach (KeyValuePair<string, RegisterTypes> currentRegister in _registerTypesMapping)
                memoryTable.Add(currentRegister.Key, _machine.Registers[currentRegister.Value].Data.ToString("X4"));
            return _printTable("Registers");
        }

        [Action("set reg")]
        public string SetReg(string regName, ushort value)
        {
            string result = "";
            //result += _printRow("Registers:", regName, _machine.Registers[_registerTypesMapping[regName]].Data.ToString("X4"));
            _machine.Registers[_registerTypesMapping[regName]].Data = value;
            result += _printRow("Registers:", regName, _machine.Registers[_registerTypesMapping[regName]].Data.ToString("X4"));
            return result;
        }

        [Action("set mem")]
        public string SetMem(ushort memAddr, ushort memValue)
        {
            string result = "";

            //result += _printMemCells("Old mem:", _machine.MainMemory, memAddr, memAddr);

            _machine.MainMemory[memAddr] = (ushort)memValue;

            result += _printMemCells("New mem:", _machine.MainMemory, memAddr, memAddr);

            return result;
        }

        [Action("set micro")]
        public string SetMicro(ushort memAddr, ushort memValue)
        {
            string result = "";

            //result += _printMemCells("Old micro:", _machine.MicrocommandsMemory, memAddr, memAddr);

            _machine.MicrocommandsMemory[memAddr] = (ushort)memValue;

            result += _printMemCells("New micro:", _machine.MicrocommandsMemory, memAddr, memAddr);

            return result;
        }

        [Action("clear mem")]
        public string ClearMem()
        {
            return ClearMem(0, (ushort)(_machine.MainMemory.CellsCount - 1));
        }

        [Action("clear micro")]
        public string ClearMicro()
        {
            return ClearMicro(0, (ushort)(_machine.MicrocommandsMemory.CellsCount - 1));
        }

        [Action("clear mem")]
        public string ClearMem(ushort startAddr, ushort endAddr)
        {
            endAddr = ((endAddr < _machine.MainMemory.CellsCount) ? endAddr : (ushort)_machine.MainMemory.CellsCount);
            for (int e = startAddr; e <= endAddr; e++)
            {
                _machine.MainMemory[e] = 0;
            }

            return String.Format("Mem {0:X4}:{1:X4} is cleared", startAddr, endAddr);
        }

        [Action("clear micro")]
        public string ClearMicro(ushort startAddr, ushort endAddr)
        {
            endAddr = ((endAddr < _machine.MicrocommandsMemory.CellsCount) ? endAddr : (ushort)_machine.MicrocommandsMemory.CellsCount);
            for (int e = startAddr; e <= endAddr; e++)
            {
                _machine.MicrocommandsMemory[e] = 0;
            }

            return String.Format("Micro {0:X4}:{1:X4} is cleared", startAddr, endAddr);
        }

        [Action("exit")]
        public string Exit()
        {
            _machineStopCallback();
            return "Good bye!";
        }

        [Action("load")]
        [Action("load mem")]
        public string LoadMemDump(string fileName)
        {
            return _loadDump(fileName, _machine.MainMemory);
        }

        [Action("load micro")]
        public string LoadMicroDump(string fileName)
        {
            return _loadDump(fileName, _machine.MicrocommandsMemory);
        }

        [Action("save")]
        [Action("save mem")]
        public string SaveMemDump(string fileName)
        {
            return _saveDump(fileName, _machine.MainMemory, 0, (ushort)_machine.MainMemory.CellsCount);
        }

        [Action("save")]
        [Action("save mem")]
        public string SaveMemDump(ushort startAddr, ushort endAddr, string fileName)
        {
            return _saveDump(fileName, _machine.MainMemory, startAddr, endAddr);
        }

        [Action("save micro")]
        public string SaveMicroDump(string fileName)
        {
            return _saveDump(fileName, _machine.MicrocommandsMemory, 0, (ushort)_machine.MicrocommandsMemory.CellsCount);
        }

        [Action("save micro")]
        public string SaveMicroDump(ushort startAddr, ushort endAddr, string fileName)
        {
            return _saveDump(fileName, _machine.MicrocommandsMemory, startAddr, endAddr);
        }

        [Action("desc micro")]
        public string DescribeMicro(ushort microcommandData)
        {
            MicroCommand microcommand = MicroCommand.Create((ushort)microcommandData);

            memoryTable.Add("Microcommand", Misc.Helper.FormatHexInt(microcommand.Source, 4));
            memoryTable.Add("Type", microcommand.Type.ToString());
            memoryTable.Add("Description", microcommand.ToString());

            return _printTable(String.Format("Microcommand:\t0x{0:X4}\n", microcommand.Source));
        }

        [Action("desc micro")]
        public string DescribeMicro(ushort startAddr, ushort endAddr)
        {
            endAddr = (ushort)((endAddr < _machine.MicrocommandsMemory.CellsCount - 1) ? endAddr : _machine.MicrocommandsMemory.CellsCount - 1);
            for (int e = startAddr; e <= endAddr; e++)
                memoryTable.Add(String.Format("{0:X4}:{1:X4}", e, _machine.MicrocommandsMemory[e]), MicroCommand.Create(_machine.MicrocommandsMemory[e]).ToString());
            return _printTable(String.Format("Micro {0:X4}:{1:X4} descriptions", startAddr, endAddr));
        }

        [Action("r")]
        [Action("run")]
        public string Run()
        {
            return StepCommand(RunLimit);
        }

        [Action("step micro")]
        public string StepMicro()
        {
            _machine.ProcessCurrentMicrocommand();
            return "Ok";
        }

        [Action("s")]
        [Action("step")]
        [Action("step command")]
        public string StepCommand()
        {
            return StepCommand(1);
        }

        [Action("s")]
        [Action("step")]
        [Action("step command")]
        public string StepCommand(uint count)
        {
            uint countResult = count;
            bool sourceRunValue = _machine.State[7];
            _machine.State[7] = true;
            while ((countResult--) > 0 && _machine.State[7])
                _machine.ProcessMicroCycle();

            string result = String.Format("Completed: {0} of {1} steps", count - countResult - 1, count);
            if (!_machine.State[7])
                result += String.Format("\nEnd of program reached");

            if (!sourceRunValue)
                _machine.State[7] = false;
            return result;
        }

        [Action("get io")]
        public string GetIO()
        {
            for (int e = 0; e < _machine.IODevices.Length; e++)
                memoryTable.Add((e + 1).ToString(), String.Format("{1} {0:X4}", _machine.IODevices[e].Data, _machine.IODevices[e].Ready));
            return _printTable("IO Devices");
        }

        [Action("get io")]
        public string GetIO(ushort index)
        {
            return _printRow(
                String.Format("IO Device #{0}", index),
                index.ToString(),
                String.Format("{1} {0:X4}", _machine.IODevices[index - 1].Data, _machine.IODevices[index - 1].Ready)
            );
        }

        [Action("set io")]
        public string SetIO(ushort index, ushort value)
        {
            _machine.IODevices[index - 1].Data = value;
            return GetIO(index);
        }

        [Action("set ioready")]
        public string SetIOReady(ushort index)
        {
            _machine.IODevices[index - 1].Ready = true;
            return GetIO(index);
        }

        [Action("unset ioready")]
        public string UnSetIOReady(ushort index)
        {
            _machine.IODevices[index - 1].Ready = false;
            return GetIO(index);
        }

        private static string _loadDump(string fileName, RAM destinationMemory)
        {
            try
            {
                XmlSerializer dumpSerializer = new XmlSerializer(typeof(MemoryDump));
                MemoryDump dump;
                using (FileStream inputStream = new FileStream(fileName, FileMode.Open))
                    dump = (MemoryDump)dumpSerializer.Deserialize(inputStream);
                destinationMemory.LoadDump(dump);
            }
            catch
            {
                return "error";
            }
            return String.Format("{0} successfully loaded", fileName);
        }

        private string _saveDump(string fileName, RAM memory, ushort start, ushort end)
        {
            try
            {
                XmlSerializer dumpSerializer = new XmlSerializer(typeof(MemoryDump));
                using (FileStream outputStream = new FileStream(fileName, FileMode.Create))
                    dumpSerializer.Serialize(outputStream, memory.Dump(start, end));
            }
            catch
            {
                return "error";
            }
            return String.Format("Dump successfully saved to {0}", fileName);
        }

        private string _printTable(string head)
        {
            StringBuilder result = new StringBuilder();
            int leftWidth = 0, rightWidth = 0;
            string formatString;
            string line = "";

            result.AppendLine(head);

            foreach (KeyValuePair<string, string> currentRow in memoryTable)
            {
                leftWidth = Math.Max(currentRow.Key.Length, leftWidth);
                rightWidth = Math.Max(currentRow.Value.Length, rightWidth);
            }

            result.Append('|');
            for (int e = 1; e <= 2 + leftWidth; e++) result.Append('-');
            result.Append('|');
            for (int e = 3 + leftWidth; e < 5 + leftWidth + rightWidth; e++) result.Append('-');
            result.Append('|');



            result.AppendLine(line);
            List<string> keys = new List<string>(memoryTable.Keys);
            for (int i = 0; i < memoryTable.Count; i++)
            {
                formatString = String.Format("{0} {{0,{1}}} {0} {{1,{2}}} {0}\n", '|', leftWidth, rightWidth);
                result.AppendFormat(formatString, keys[i], memoryTable[keys[i]]);

                result.Append('|');
                for (int e = 1; e <= 2 + leftWidth; e++) result.Append('-');
                result.Append('|');
                for (int e = 3 + leftWidth; e < 5 + leftWidth + rightWidth; e++) result.Append('-');
                result.Append('|');
                result.AppendLine();
            }
            memoryTable.Clear();
            return result.ToString();
        }

        private string _printRow(string head, string address, string value)
        {
            memoryTable.Add(address, value);
            return _printTable(head);
        }

        private string _printMemCells(string head, Memory.RAM memory, ushort startIndex, ushort endIndex)
        {
            for (ushort e = startIndex; e <= endIndex; e++)
                if (e >= 0 && e < memory.CellsCount)
                    memoryTable.Add(
                        Misc.Helper.FormatHexInt(e, 3),
                        Misc.Helper.FormatHexInt(memory[e], 4));
            return _printTable(head);
        }

        //private string _printWatches()
        //{

        //}
    }
}
