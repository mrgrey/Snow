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
    /// Описывает ОМК1(Операционную Микрокоманду 1)
    /// </summary>
    [CLSCompliant(false)]
    public class OperationalMicrocommand1 : MicroCommand
    {
        /// <summary>
        /// Определяет операцию над регистром сдвига
        /// </summary>
        public readonly CarryRegisterValueOperationType CarryRegisterValueOperation;

        /// <summary>
        /// Определяет операции над регистрами N и Z
        /// </summary>
        public readonly NZRegistersValueOperationType NZRegistersValueOperation;

        /// <summary>
        /// Определяет операцию над БР 
        /// </summary>
        public readonly AluOutputAcceptor AluOutputOperation;

        /// <summary>
        /// Определяет операции ввода-вывода
        /// </summary>
        public readonly IOOperations IOOperation;

        /// <summary>
        /// Производится ли останов машины
        /// </summary>
        public readonly bool Halt;

        /// <summary>
        /// Конструктор по умолчанию. Создает ОМК1 c установленным типом микрокоманды и "пустыми" полями
        /// (значения по умолчанию)
        /// </summary>
        public OperationalMicrocommand1() : this(0x4000) { }

        /// <summary>
        /// Конструктор. Создает ОМК1 согласно переданному коду. В случае если переданный код
        /// соответствует команде другого типа конструктор вызывает исключение ArgumentException.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// В конструктор передан код соответствующий другому типу микрокоманды
        /// </exception>
        /// <param name="source">Исходный код микрокоманды</param>
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
        /// Возвращает мнемоническое представление микрокоманды
        /// </summary>
        /// <returns>Строку, содержащую мнемоническое представление микрокоманды</returns>
        public override string ToString()
        {
            string sendString = "", shiftRegisterActionString = "", haltString = "", ioString = "";

            switch (AluOutputOperation)
            {
                case AluOutputAcceptor.ToAccumulator:
                    sendString += "А";
                    break;
                case AluOutputAcceptor.ToAddressRegister:
                    sendString += "РА";
                    break;
                case AluOutputAcceptor.ToCommandRegister:
                    sendString += "РК";
                    break;
                case AluOutputAcceptor.ToCommandCounter:
                    sendString += "СК";
                    break;
                case AluOutputAcceptor.ToDataRegister:
                    sendString += "РД";
                    break;
                case AluOutputAcceptor.ToAllWithoutCommandCounter:
                    sendString += "А, РА, РД, РК";
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
                ioString += "В/В";
            else if ((IOOperation & IOOperations.ClearIOFlags) != 0)
                ioString = "Очищение флагов В/В";
            else if ((IOOperation & IOOperations.DisableInterrupt) != 0)
                ioString = "Запрещение прерывания";
            else if ((IOOperation & IOOperations.EnableInterrupt) != 0)
                ioString = "Разрешение прерывания";

            if (sendString.Length != 0)
                sendString = "БР ==> " + sendString;

            return Helper.JoinNotEmptyStrings(sendString, shiftRegisterActionString, ioString, haltString);
        }

        /// <summary>
        /// Возвращает тип микрокоманды
        /// </summary>
        public override MicroCommandType Type
        {
            get { return MicroCommandType.Operational1; }
        }


    }
}
