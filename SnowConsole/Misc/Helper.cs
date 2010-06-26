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

namespace Cleancode.Snow.Misc
{
    /// <summary>
    /// ����� �������� ��������������� ������
    /// </summary>
    public sealed class Helper
    {
        private Helper() { }
        /// <summary>
        /// ���������� ������������ �������� ����� � �������������� ����������� ", "
        /// </summary>
        /// <param name="strings">������</param>
        /// <returns>��������� ������������</returns>
        public static string JoinNotEmptyStrings(params string[] strings)
        {
            StringBuilder result = new StringBuilder();
            for (int e = 0; e < strings.Length; e++)
            {
                if (strings[e].Length != 0)
                {
                    result.Append(strings[e]);
                    if (e + 1 < strings.Length && strings[e + 1].Length != 0)
                        result.Append(", ");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// ������� ������� ����� ��������� ����� (������� �� �������� ���� - 00011111)
        /// </summary>
        /// <param name="length">����� ����� � �����</param>
        /// <returns>����������� �����</returns>
        public static int GetMaskByLength(byte length)
        {
            return (short)(((1 << (--length)) - 1) | (1 << length));
        }

        /// <summary>
        /// ����������� ���������� ������ � �������� ������������� (������ � �����)
        /// </summary>
        /// <typeparam name="T">��� �������� ��������� �������</typeparam>
        /// <param name="source">�������� ������</param>
        /// <returns>�������� ������</returns>
        public static byte[] GetDumpByArray<T>(T[] source) where T : struct
        {
            byte[] dump = new byte[Buffer.ByteLength(source)];
            Buffer.BlockCopy(source, 0, dump, 0, dump.Length);
            return dump;
        }


        /// <summary>
        /// ������ ����� (int) � ����������������� �������
        /// </summary>
        /// <param name="inputString">������ ���������� ����� � ����������������� �������</param>
        /// <returns>����� ���������������</returns>
        public static int ParseHexNumber(string inputString)
        {
            if (inputString.StartsWith("0x"))
                inputString = inputString.Substring(2);
            return int.Parse(inputString, System.Globalization.NumberStyles.AllowHexSpecifier);
        }

        public static string FormatHexInt(int inputNumber, int width)
        {
            return String.Format(String.Format("{{0:X{0}}}", width), inputNumber);
        }
    }
}
