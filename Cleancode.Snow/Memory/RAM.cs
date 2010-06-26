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
using System.IO;

namespace Cleancode.Snow.Memory
{
    //–еализаци€ RAM
    public class RAM
    {
        bool _needCellChangeInvoke = true;
        List<DataCell> _cells;

        public DataCellEventHandler CellChangedHandler;

        protected RAM() { }

        /// <summary>
        /// —оздает набор €чеек RAM пам€ти указанной длины
        /// </summary>
        /// <param name="cellsCount">ƒлина набора €чеек пам€ти</param>
        private void _createCellsByCount(int cellsCount)
        {
            if (cellsCount <= 0)
                throw new ArgumentException("Unsupported cells count", "cellsCount");
            if (_cells.Count != 0)
                throw new NotSupportedException("Cannot change cells count");
            for (int e = 0; e < cellsCount; e++)
                _cells.Add(new MemoryCell(16, e));
        }


        private void _cellChanged(DataCell changedCell)
        {
            if (_needCellChangeInvoke)
                if (CellChangedHandler != null)
                    CellChangedHandler(changedCell);
        }

        /// <summary>
        ///  онструктор, создающий набор €чеек RAM пам€ти указанной длины
        /// </summary>
        /// <param name="cellsCount">ƒлина набора €чеек пам€ти</param>
        public RAM(int cellsCount)
        {
            _cells = new List<DataCell>(cellsCount);
            _createCellsByCount(cellsCount);
        }

        public void LoadDump(MemoryDump dump)
        {
            ushort cellsOffset = (ushort)Misc.Helper.ParseHexNumber(dump.CellsOffset);
            Dictionary<ushort, ushort> dumpSource = new Dictionary<ushort, ushort>();
            ushort address;

            for (int e = 0; e < dump.Cells.Length; e++)
            {
                address = (ushort)Misc.Helper.ParseHexNumber(dump.Cells[e].Address);
                if (address >= this._cells.Count)
                    throw new ArgumentException("Cannot load dump");
                dumpSource.Add(address, (ushort)Misc.Helper.ParseHexNumber(dump.Cells[e].Value));
            }

            foreach (KeyValuePair<ushort, ushort> currentCell in dumpSource)
                this._cells[currentCell.Key].Data = currentCell.Value;
        }


        //»ндексатор предоставл€ющий доступ к €чейкам пам€ти по номеру
        public ushort this[int index]
        {
            get
            {
                if (index >= _cells.Count || index < 0)
                    throw new ArgumentException("Incorect cell index", "index");
                return _cells[index].Data;
            }
            set
            {
                if (index >= _cells.Count || index < 0)
                    throw new ArgumentException("Incorect cell index", "index");
                _cells[index].Data = value;
            }
        }

        /// <summary>
        /// ¬озвращает дамп пригодный дл€ сериализации
        /// </summary>
        /// <returns></returns>
        public MemoryDump Dump(ushort startAddr, ushort endAddr)
        {
            MemoryDump result = new MemoryDump();
            result.CellsOffset = "0";
            result.Cells = new DumpMemoryCell[_cells.Count];
            for (
                int e = startAddr;
                e <= ((endAddr < _cells.Count - 1) ? endAddr : _cells.Count - 1);
                e++)
                result.Cells[e] = new DumpMemoryCell(
                    String.Format("{0:X3}", e),
                    String.Format("{0:X4}", _cells[e].Data));
            return result;
        }

        public int CellsCount
        { get { return _cells.Count; } }
    }
}
