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

namespace Cleancode.Snow.IO
{
    /// <summary>
    /// ��������� �������� ����������� ������� ���������
    /// </summary>
    [Flags]
    public enum IOOperations
    {
        None = 0,
        /// <summary>
        /// ����� �������
        /// </summary>
        ProcessIO = 1,
        /// <summary>
        /// ����� ������ ������� ���������
        /// </summary>
        ClearIOFlags = 2,
        /// <summary>
        /// ���������� ����������
        /// </summary>
        DisableInterrupt = 4,
        /// <summary>
        /// ���������� ����������
        /// </summary>
        EnableInterrupt = 8,
    }
}
