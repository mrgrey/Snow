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
    /// Представляет ячейку памяти заданного размера (<=16 бит)
    /// </summary>
    public abstract class DataCell
    {
        /// <summary>
        /// Значение ячейки
        /// </summary>
        ushort _data;
        /// <summary>
        /// Длина значения ячейки
        /// </summary>
        public readonly byte DataLength;
        /// <summary>
        /// Поле, используемое для идентификации ячейки
        /// </summary>
        public object Id;


        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        protected DataCell()
        { }

        /// <summary>
        /// Конструктор, создает экземпляр с заданными размером и идентификатором<
        /// </summary>
        /// <param name="length">Размер ячейки в битах (<=16)</param>
        /// <param name="id">Идентификатор ячейки</param>
        protected DataCell(byte length, object id)
        {
            //ячейка не может быть нулевого размера и больше чем 2 байта (16 бит)
            if (length == 0 || length > 16)
                throw new ArgumentException("Unsupported cell length");
            _data = 0;
            DataLength = length;
            Id = id;
        }

        /// <summary>
        /// Возвращает тип ячейки
        /// </summary>
        public abstract DataCellType Type
        { get;}

        /// <summary>
        /// Возвращает и устанавливает данные ячейки с учетом длины и инициирует соответствующие события
        /// </summary>
        public ushort Data
        {
            get
            {
                return _data;
            }
            set
            {
                //применяем к полученным данным маску, обрезая не укладывающиеся в текущий размер ячейки
                ushort tempData = (ushort)(value & Helper.GetMaskByLength(DataLength));
                _data = tempData;
            }
        }

        /// <summary>
        /// Возвращает или устанавливает бит значения ячейки с указанным индексом
        /// </summary>
        /// <param name="index">Индекс бита в значении, который нужно вернуть</param>
        /// <returns>Бит с указанным индексом или false если индекс "вылетает" за границы ячейки, но лежит в диапазоне 2х байт</returns>
        public bool this[int index]
        {
            get
            {
                //если индекс "вылетает" за границы 2х байт, то инициируется соответствующее исключение
                if (index < 0 || index >= 16)
                    throw new ArgumentException("Unsupported bit index");
                /* Возвращаем значение бита с индексом index.
                 * Проверка на попадание индекса в указанный размер ячейки не производится вполне осознанно
                 * */
                return (_data & (1 << index)) != 0;
            }
            set
            {
                /*
                 * Несмотря на то, что конкретный регистр 
                 * может быть короче слова (16 бит) мы проверяем индекс бита лишь на 
                 * соответствие максимальному индексу для слова
                 */

                if (index < 0 || index >= 16)
                    throw new ArgumentException("Unsupported bit index");
                if (index >= DataLength)
                    return;
                //получаем текущее значение бита с указанным индексом 
                //TODO и провоцируем вызов OnDataReaded!!!
                bool currentValue = this[index];
                //сравниваем текущее значение с полученным, если различаются, то меняем
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
