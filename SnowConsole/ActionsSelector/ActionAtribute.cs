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

namespace Cleancode.Snow
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    // Атрибут действия
    class ActionAttribute : Attribute
    {
        string _description;
        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        //Массив подстрок входной строки. Подстроки получены разделением
        //входной строки по символу ' '
        string[] _cmdParts;

        public string[] CmdParts
        {
            get { return _cmdParts; }
        }

        /// <summary>
        /// Возвращает строку используемую для инстанциирования объекта (входную строку)
        /// </summary>
        public string Cmd
        {
            get { return String.Join(" ", _cmdParts); }
        }

        protected ActionAttribute() { }
        /// <summary>
        /// Конструктор создающий экземпляр класса и заполняющий массив _cmdParts данными из входной
        /// строки, разделенной по символу ' '
        /// </summary>
        /// <param name="cmd">Входная строка</param>
        public ActionAttribute(string cmd)
        {
            _cmdParts = cmd.Split(' ');
        }
    }
}
