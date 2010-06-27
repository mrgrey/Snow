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
using System.Linq;

namespace Cleancode.Snow.Misc
{
    /// <summary>
    /// Класс содержит вспомогательные методы
    /// </summary>
    public sealed class Helper
    {
        private Helper() { }
        /// <summary>
        /// Производит конкатенацию непустых строк с использованием разделителя ", "
        /// </summary>
        /// <param name="strings">Строки</param>
        /// <returns>Результат конкатенации</returns>
        public static string JoinNotEmptyStrings(params string[] strings)
        {
            string separator = ", ";
            return String.Join(
                separator, 
                strings.Where(s => s.Length != 0).ToArray()
            );
        }

        /// <summary>
        /// Создает битовую маску указанной длины (считаем от младшего бита - 00011111)
        /// </summary>
        /// <param name="length">Длина маски в битах</param>
        /// <returns>Вычисленная маска</returns>
        public static ushort GetMaskByLength(byte length)
        {
            if (length > 16)
                throw new ArgumentOutOfRangeException("length");

            return (ushort)(((1 << (--length)) - 1) | (1 << length));
        }

        /// <summary>
        /// Преобразует полученный массив в байтовое представление (дампит в общем)
        /// </summary>
        /// <typeparam name="T">Тип элемента исходного массива</typeparam>
        /// <param name="source">Исходный массив</param>
        /// <returns>Байтовый массив</returns>
        public static byte[] GetDumpByArray<T>(T[] source) where T : struct
        {
            byte[] dump = new byte[Buffer.ByteLength(source)];
            Buffer.BlockCopy(source, 0, dump, 0, dump.Length);
            return dump;
        }


        /// <summary>
        /// Парсит число (int) в шестнадцатеричном формате
        /// </summary>
        /// <param name="inputString">Строка содержащая число в шестнадцатеричном формате</param>
        /// <returns>Число соответствующее</returns>
        public static int ParseHexNumber(string inputString)
        {
            try
            {
                if (inputString.StartsWith("0x"))
                    inputString = inputString.Substring(2);

                return int.Parse(inputString, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch (FormatException)
            {
                throw new ArgumentException("inputString is not a correct hexadecimal string", "inputString"); 
            }
        }

        public static string FormatHexInt(uint inputNumber, int width)
        {
            var formatString = String.Format("{{0:X{0}}}", width);
            return String.Format(formatString, inputNumber);
        }
    }
}
