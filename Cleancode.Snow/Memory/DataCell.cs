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
using Cleancode.Snow.Misc;

namespace Cleancode.Snow.Memory
{
    public delegate void DataCellEventHandler(DataCell sender);

    /// <summary>
    /// ѕредставл€ет €чейку пам€ти заданного размера (<=16 бит)
    /// </summary>
    public class DataCell : Cleancode.Snow.Memory.IDataCell
    {
        /// <summary>
        /// «начение €чейки
        /// </summary>
        ushort _data;
        /// <summary>
        /// «начение €чейки
        /// </summary>
        ushort _readonlyMask;
        /// <summary>
        /// ƒлина значени€ €чейки
        /// </summary>
        public readonly byte DataLength;
        /// <summary>
        /// ѕоле, используемое дл€ идентификации €чейки
        /// </summary>
        public object Id;


        /// <summary>
        ///  онструктор по умолчанию
        /// </summary>
        private DataCell()
        { }

        /// <summary>
        ///  онструктор, создает экземпл€р с заданными размером и идентификатором<
        /// </summary>
        /// <param name="length">–азмер €чейки в битах (<=16)</param>
        /// <param name="id">»дентификатор €чейки</param>
        public  DataCell(byte length, object id)
        {
            //€чейка не может быть нулевого размера и больше чем 2 байта (16 бит)
            if (length == 0 || length > 16)
                throw new ArgumentOutOfRangeException("Unsupported cell length");
            _data = 0;
            DataLength = length;
            Id = id;
            _readonlyMask = 0;
        }

        public DataCell(byte length, object id, ushort defaultValue)
            : this(length, id)
        {
            _data = (ushort)(defaultValue & Helper.GetMaskByLength(DataLength));
        }

        public DataCell(byte length, object id, ushort defaultValue, ushort readonlyMask)
            : this(length, id, defaultValue)
        {
            _readonlyMask = readonlyMask;
        }

        /// <summary>
        /// ¬озвращает тип €чейки
        /// </summary>
        public virtual DataCellType Type
        {
            get
            {
                return DataCellType.Unknown;
            }
        }

        /// <summary>
        /// ¬озвращает и устанавливает данные €чейки с учетом длины и инициирует соответствующие событи€
        /// </summary>
        public virtual ushort Data
        {
            get
            {
                return _data;
            }
            set
            {
                //примен€ем к полученным данным маску, обреза€ не укладывающиес€ в текущий размер €чейки
                ushort tempData = (ushort)(value & Helper.GetMaskByLength(DataLength));
                tempData &= (ushort)(~_readonlyMask);
                _data = tempData;
            }
        }

        /// <summary>
        /// ¬озвращает или устанавливает бит значени€ €чейки с указанным индексом
        /// </summary>
        /// <param name="index">»ндекс бита в значении, который нужно вернуть</param>
        /// <returns>Ѕит с указанным индексом или false если индекс "вылетает" за границы €чейки, но лежит в диапазоне 2х байт</returns>
        public virtual bool this[int index]
        {
            get
            {
                //если индекс "вылетает" за границы 2х байт, то инициируетс€ соответствующее исключение
                if (index < 0 || index >= 16)
                    throw new ArgumentException("Unsupported bit index");
                /* ¬озвращаем значение бита с индексом index.
                 * ѕроверка на попадание индекса в указанный размер €чейки не производитс€ вполне осознанно
                 * */
                return (_data & (1 << index)) != 0;
            }
            set
            {
                /*
                 * Ќесмотр€ на то, что конкретный регистр 
                 * может быть короче слова (16 бит) мы провер€ем индекс бита лишь на 
                 * соответствие максимальному индексу дл€ слова
                 */

                if (index < 0 || index >= 16)
                    throw new ArgumentException("Unsupported bit index");
                if (index >= DataLength)
                    return;
                if ((1 << index & _readonlyMask) != 0)
                    return;
                //получаем текущее значение бита с указанным индексом 
                //TODO и провоцируем вызов OnDataReaded!!!
                bool currentValue = this[index];
                //сравниваем текущее значение с полученным, если различаютс€, то мен€ем
                if (currentValue != value)
                {
                    if (value)
                        _data |= (ushort)(1 << index);
                    else
                        unchecked
                        {
                            _data &= (ushort)~(1 << index);
                        }
                }
            }
        }
    }
}
