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

namespace Cleancode.Snow.Microcommands
{
    /// <summary>
    /// Описывает УМК(Управляющую Микрокоманду)
    /// </summary>
    public class ControlMicroCommand : MicroCommand
    {
        /// <summary>
        /// Конструктор по умолчанию. Создает УМК c установленным типом микрокоманды и "пустыми" полями
        /// </summary>
        public ControlMicroCommand() : this(0x8000) { }

        /// <summary>
        /// Конструктор. Создает УМК согласно переданному коду. В случае если переданный код
        /// соответствует команде другого типа конструктор вызывает исключение ArgumentException.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// В конструктор передан код соответствующий другому типу микрокоманды
        /// </exception>
        /// <param name="source">Исходный код микрокоманды</param>
        public ControlMicroCommand(ushort source)
            : base(source)
        {
            if ((source & 0x8000) == 0)
                throw new ArgumentException("Unknown command type", "source");
            JumpAddress = (ushort)(Source & 0x00FF);
            TestBitIndex = (ushort)((Source >> 8) & 0x000F);
            ArgumentSource = (ControlMCArgumentSource)((Source >> 12) & 0x0003);
            Condition = ((Source & 0x4000) != 0);
        }

        /// <summary>
        /// Возвращает мнемоническое представление микрокоманды
        /// </summary>
        /// <returns>Строку, содержащую мнемоническое представление микрокоманды</returns>
        public override string ToString()
        {
            string registerName = "", result;
            switch (ArgumentSource)
            {
                case ControlMCArgumentSource.Accumulator:
                    registerName = "А";
                    break;
                case ControlMCArgumentSource.CommandRegister:
                    registerName = "РК";
                    break;
                case ControlMCArgumentSource.DataRegister:
                    registerName = "РД";
                    break;
                case ControlMCArgumentSource.StatesRegister:
                    registerName = "РС";
                    break;
            }

            result = String.Format("GOTO ({0,2:X})", JumpAddress);

            if (ArgumentSource != ControlMCArgumentSource.StatesRegister || TestBitIndex != 3)
                result = String.Format("IF BIT({0},{1})={2} THEN {3}", TestBitIndex, registerName, Condition ? 1 : 0, result);

            return result;
        }

        /// <summary>
        /// Возвращает тип микрокоманды
        /// </summary>
        public override MicroCommandType Type
        {
            get { return MicroCommandType.Control; }
        }

        public readonly ControlMCArgumentSource ArgumentSource;

        public readonly ushort TestBitIndex;

        public readonly ushort JumpAddress;

        public readonly bool Condition;

    }
}
