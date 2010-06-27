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
    /// ������������ ������ ������ ��������� ������� (<=16 ���)
    /// </summary>
    public abstract class DataCell
    {
        /// <summary>
        /// �������� ������
        /// </summary>
        ushort _data;
        /// <summary>
        /// ����� �������� ������
        /// </summary>
        public readonly byte DataLength;
        /// <summary>
        /// ����, ������������ ��� ������������� ������
        /// </summary>
        public object Id;


        /// <summary>
        /// ����������� �� ���������
        /// </summary>
        protected DataCell()
        { }

        /// <summary>
        /// �����������, ������� ��������� � ��������� �������� � ���������������<
        /// </summary>
        /// <param name="length">������ ������ � ����� (<=16)</param>
        /// <param name="id">������������� ������</param>
        protected DataCell(byte length, object id)
        {
            //������ �� ����� ���� �������� ������� � ������ ��� 2 ����� (16 ���)
            if (length == 0 || length > 16)
                throw new ArgumentException("Unsupported cell length");
            _data = 0;
            DataLength = length;
            Id = id;
        }

        /// <summary>
        /// ���������� ��� ������
        /// </summary>
        public abstract DataCellType Type
        { get;}

        /// <summary>
        /// ���������� � ������������� ������ ������ � ������ ����� � ���������� ��������������� �������
        /// </summary>
        public ushort Data
        {
            get
            {
                return _data;
            }
            set
            {
                //��������� � ���������� ������ �����, ������� �� �������������� � ������� ������ ������
                ushort tempData = (ushort)(value & Helper.GetMaskByLength(DataLength));
                _data = tempData;
            }
        }

        /// <summary>
        /// ���������� ��� ������������� ��� �������� ������ � ��������� ��������
        /// </summary>
        /// <param name="index">������ ���� � ��������, ������� ����� �������</param>
        /// <returns>��� � ��������� �������� ��� false ���� ������ "��������" �� ������� ������, �� ����� � ��������� 2� ����</returns>
        public bool this[int index]
        {
            get
            {
                //���� ������ "��������" �� ������� 2� ����, �� ������������ ��������������� ����������
                if (index < 0 || index >= 16)
                    throw new ArgumentException("Unsupported bit index");
                /* ���������� �������� ���� � �������� index.
                 * �������� �� ��������� ������� � ��������� ������ ������ �� ������������ ������ ���������
                 * */
                return (_data & (1 << index)) != 0;
            }
            set
            {
                /*
                 * �������� �� ��, ��� ���������� ������� 
                 * ����� ���� ������ ����� (16 ���) �� ��������� ������ ���� ���� �� 
                 * ������������ ������������� ������� ��� �����
                 */

                if (index < 0 || index >= 16)
                    throw new ArgumentException("Unsupported bit index");
                if (index >= DataLength)
                    return;
                //�������� ������� �������� ���� � ��������� �������� 
                //TODO � ����������� ����� OnDataReaded!!!
                bool currentValue = this[index];
                //���������� ������� �������� � ����������, ���� �����������, �� ������
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
