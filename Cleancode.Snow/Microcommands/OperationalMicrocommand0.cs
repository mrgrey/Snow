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
using Cleancode.Snow.Misc;

namespace Cleancode.Snow.Microcommands
{
    /// <summary>
    /// Описывает ОМК0(Операционную Микрокоманду 0)
    /// </summary>
    public class OperationalMicrocommand0 : MicroCommand
    {
        /// <summary>
        /// Источник данных для левого входа АЛУ
        /// </summary>
        public AluLeftInputSource AluLeftInput;

        /// <summary>
        /// Источник данных для правого входа АЛУ
        /// </summary>
        public AluRightInputSource AluRightInput;

        /// <summary>
        /// Определяет на каком входе АЛУ будет произведено инвертирование значения
        /// </summary>
        public AluInputInvertMode AluInputInvert;

        /// <summary>
        /// Определяет операцию, выполняемую АЛУ
        /// </summary>
        public AluOperationType AluOperation;

        /// <summary>
        /// Определяет сдвиг аккумулятора
        /// </summary>
        public RotateAccumulatorMode RotateAccumulator;

        /// <summary>
        /// Определяет операцию обмена с оперативной памятью
        /// </summary>
        public MemoryOperationType MemoryOperation;



        /// <summary>
        /// Конструктор по умолчанию. Создает ОМК0 c установленным типом микрокоманды и "пустыми" полями
        /// (значения по умолчанию)
        /// </summary>
        public OperationalMicrocommand0() : this(0) { }

        /// <summary>
        /// Конструктор. Создает ОМК0 согласно переданному коду. В случае если переданный код
        /// соответствует команде другого типа конструктор вызывает исключение ArgumentException.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// В конструктор передан код соответствующий другому типу микрокоманды
        /// </exception>
        /// <param name="source">Исходный код микрокоманды</param>
        public OperationalMicrocommand0(ushort source)
            : base(source)
        {
            MemoryOperation = (MemoryOperationType)(Source & 0x0003);
            RotateAccumulator = (RotateAccumulatorMode)((Source >> 2) & 0x0003);
            AluOperation = (AluOperationType)((Source >> 4) & 0x0003);
            AluInputInvert = (AluInputInvertMode)((Source >> 6) & 0x0003);
            AluRightInput = (AluRightInputSource)((Source >> 8) & 0x0003);
            AluLeftInput = (AluLeftInputSource)((Source >> 12) & 0x0003);

            if (((Source >> 14) & 0x0003) != 0)
                throw new ArgumentException("Unknown command type", "source");
        }

        /// <summary>
        /// Возвращает мнемоническое представление микрокоманды
        /// </summary>
        /// <returns>Строку, содержащую мнемоническое представление микрокоманды</returns>
        public override string ToString()
        {
            string leftAluOperand = "0", rightAluOperand = "0";
            string aluString = "", memoryString = "", shiftString = "";
            switch (AluLeftInput)
            {
                case AluLeftInputSource.AccumulatorRegister:
                    leftAluOperand = "А";
                    break;
                case AluLeftInputSource.KeyboardRegister:
                    leftAluOperand = "КР";
                    break;
                case AluLeftInputSource.StatesRegister:
                    leftAluOperand = "РС";
                    break;
            }
            switch (AluRightInput)
            {
                case AluRightInputSource.CommandRegister:
                    rightAluOperand = "РК";
                    break;
                case AluRightInputSource.CommandsCounterRegister:
                    rightAluOperand = "СК";
                    break;
                case AluRightInputSource.DataRegister:
                    rightAluOperand = "РД";
                    break;
            }
            switch (AluInputInvert)
            {
                case AluInputInvertMode.Left:
                    leftAluOperand = "COM(" + leftAluOperand + ")";
                    break;
                case AluInputInvertMode.Right:
                    rightAluOperand = "COM(" + rightAluOperand + ")";
                    break;
            }

            if (AluOperation == AluOperationType.LogicalAnd)
            {
                if (AluLeftInput != AluLeftInputSource.Zero && AluRightInput != AluRightInputSource.Zero)
                    aluString = leftAluOperand + " & " + rightAluOperand;
                else
                    aluString = "0";
            }
            else
            {
                if (leftAluOperand != "0")
                {
                    aluString = leftAluOperand;
                    if (rightAluOperand != "0")
                        aluString += " + ";
                }
                if (rightAluOperand != "0")
                    aluString += rightAluOperand;
                if (AluOperation == AluOperationType.SummAndOne)
                {
                    if (aluString.Length != 0)
                        aluString += " + ";
                    aluString += "1";
                }

            }


            if (aluString.Length != 0)
                aluString += " ==> БР";

            if (MemoryOperation != MemoryOperationType.None)
                if (MemoryOperation == MemoryOperationType.Read)
                    memoryString = "RAM(РА) ==> РД";
                else
                    memoryString = "РД ==> RAM(РА)";

            if (RotateAccumulator != RotateAccumulatorMode.None)
                if (RotateAccumulator == RotateAccumulatorMode.Left)
                    shiftString = "RAL(А) ==> БР";
                else
                    shiftString = "RAR(А) ==> БР";

            return Helper.JoinNotEmptyStrings(memoryString, aluString, shiftString);
        }


        /// <summary>
        /// Возвращает тип микрокоманды
        /// </summary>
        public override MicroCommandType Type
        {
            get { return MicroCommandType.Operational0; }
        }


    }
}
