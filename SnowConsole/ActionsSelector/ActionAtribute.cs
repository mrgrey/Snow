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
    // ������� ��������
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

        //������ �������� ������� ������. ��������� �������� �����������
        //������� ������ �� ������� ' '
        string[] _cmdParts;

        public string[] CmdParts
        {
            get { return _cmdParts; }
        }

        /// <summary>
        /// ���������� ������ ������������ ��� ���������������� ������� (������� ������)
        /// </summary>
        public string Cmd
        {
            get { return String.Join(" ", _cmdParts); }
        }

        protected ActionAttribute() { }
        /// <summary>
        /// ����������� ��������� ��������� ������ � ����������� ������ _cmdParts ������� �� �������
        /// ������, ����������� �� ������� ' '
        /// </summary>
        /// <param name="cmd">������� ������</param>
        public ActionAttribute(string cmd)
        {
            _cmdParts = cmd.Split(' ');
        }
    }
}
