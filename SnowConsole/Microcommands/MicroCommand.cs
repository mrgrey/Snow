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
    /// Базовый класс и фабрика микрокоманд
    /// </summary>
    [CLSCompliant(false)]
    public abstract class MicroCommand
    {
        public readonly ushort Source;

        /// <summary>
        /// Возвращает тип микрокоманды
        /// </summary>
        public abstract MicroCommandType Type { get;}

        /// <summary>
        /// Создает и заполняет микрокоманду соответствующую переданному коду.
        /// </summary>
        /// <param name="source">Исходный код микрокоманды, по которому ведется конструирование</param>
        /// <returns>Сконструированную микрокоманду</returns>
        [CLSCompliant(false)]
        public static MicroCommand Create(ushort source)
        {
            MicroCommand result = null;

            //читаем первые 2 бита команды для определения ее типа
            if ((source & 0x8000) == 0)
                if ((source & 0x4000) == 0)
                    result = new OperationalMicrocommand0(source);
                else
                    result = new OperationalMicrocommand1(source);
            else
                result = new ControlMicroCommand(source);
            return result;
        }

        protected MicroCommand(ushort source)
        {
            Source = source;
        }
    }
}
