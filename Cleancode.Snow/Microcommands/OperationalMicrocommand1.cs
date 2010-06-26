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
using System.Collections.Specialized;
using System.Text;
using Cleancode.Snow.ALU;
using Cleancode.Snow.IO;
using Cleancode.Snow.Misc;

namespace Cleancode.Snow.Microcommands
{
    /// <summary>
    /// ��������� ���1(������������ ������������ 1)
    /// </summary>
    [CLSCompliant(false)]
    public class OperationalMicrocommand1 : MicroCommand
    {
        /// <summary>
        /// ���������� �������� ��� ��������� ������
        /// </summary>
        public readonly CarryRegisterValueOperationType CarryRegisterValueOperation;

        /// <summary>
        /// ���������� �������� ��� ���������� N � Z
        /// </summary>
        public readonly NZRegistersValueOperationType NZRegistersValueOperation;

        /// <summary>
        /// ���������� �������� ��� �� 
        /// </summary>
        public readonly AluOutputAcceptor AluOutputOperation;

        /// <summary>
        /// ���������� �������� �����-������
        /// </summary>
        public readonly IOOperations IOOperation;

        /// <summary>
        /// ������������ �� ������� ������
        /// </summary>
        public readonly bool Halt;

        /// <summary>
        /// ����������� �� ���������. ������� ���1 c ������������� ����� ������������ � "�������" ������
        /// (�������� �� ���������)
        /// </summary>
        public OperationalMicrocommand1() : this(0x4000) { }

        /// <summary>
        /// �����������. ������� ���1 �������� ����������� ����. � ������ ���� ���������� ���
        /// ������������� ������� ������� ���� ����������� �������� ���������� ArgumentException.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// � ����������� ������� ��� ��������������� ������� ���� ������������
        /// </exception>
        /// <param name="source">�������� ��� ������������</param>
        public OperationalMicrocommand1(ushort source)
            : base(source)
        {
            AluOutputOperation = (AluOutputAcceptor)(Source & 0x0007);
            Halt = ((Source >> 3) & 0x0001) > 0;
            NZRegistersValueOperation = (NZRegistersValueOperationType)((Source >> 4) & 0x0003);
            CarryRegisterValueOperation = (CarryRegisterValueOperationType)((Source >> 6) & 0x0003);
            IOOperation = (IOOperations)((Source >> 8) & 0x000F);
            if (((Source >> 14) & 0x0003) != 1)
                throw new ArgumentException("Unknown command type", "source");
        }

        /// <summary>
        /// ���������� ������������� ������������� ������������
        /// </summary>
        /// <returns>������, ���������� ������������� ������������� ������������</returns>
        public override string ToString()
        {
            string sendString = "", shiftRegisterActionString = "", haltString = "", ioString = "";

            switch (AluOutputOperation)
            {
                case AluOutputAcceptor.ToAccumulator:
                    sendString += "�";
                    break;
                case AluOutputAcceptor.ToAddressRegister:
                    sendString += "��";
                    break;
                case AluOutputAcceptor.ToCommandRegister:
                    sendString += "��";
                    break;
                case AluOutputAcceptor.ToCommandCounter:
                    sendString += "��";
                    break;
                case AluOutputAcceptor.ToDataRegister:
                    sendString += "��";
                    break;
                case AluOutputAcceptor.ToAllWithoutCommandCounter:
                    sendString += "�, ��, ��, ��";
                    break;
            }

            switch (CarryRegisterValueOperation)
            {
                case CarryRegisterValueOperationType.SetByBufferRegister:
                    if (sendString.Length != 0)
                        sendString += ", ";
                    sendString += "C";
                    break;
                case CarryRegisterValueOperationType.Clear:
                    shiftRegisterActionString = "0 ==> C";
                    break;
                case CarryRegisterValueOperationType.Set:
                    shiftRegisterActionString = "1 ==> C";
                    break;
            }

            if ((NZRegistersValueOperation & NZRegistersValueOperationType.SetN) != 0)
            {
                if (sendString.Length != 0)
                    sendString += ", ";
                sendString += "N";
            }
            if ((NZRegistersValueOperation & NZRegistersValueOperationType.SetZ) != 0)
            {
                if (sendString.Length != 0)
                    sendString += ", ";
                sendString += "Z";
            }

            if (Halt)
                haltString = "HLT";

            if ((IOOperation & IOOperations.ProcessIO) != 0)
                ioString += "�/�";
            else if ((IOOperation & IOOperations.ClearIOFlags) != 0)
                ioString = "�������� ������ �/�";
            else if ((IOOperation & IOOperations.DisableInterrupt) != 0)
                ioString = "���������� ����������";
            else if ((IOOperation & IOOperations.EnableInterrupt) != 0)
                ioString = "���������� ����������";

            if (sendString.Length != 0)
                sendString = "�� ==> " + sendString;

            return Helper.JoinNotEmptyStrings(sendString, shiftRegisterActionString, ioString, haltString);
        }

        /// <summary>
        /// ���������� ��� ������������
        /// </summary>
        public override MicroCommandType Type
        {
            get { return MicroCommandType.Operational1; }
        }


    }
}
