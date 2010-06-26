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
using System.Reflection;
using Cleancode.Snow.Microcommands;
using Cleancode.Snow.Memory;
using Cleancode.Snow.Core;
using System.IO;
using System.Xml.Serialization;

namespace Cleancode.Snow.FirstTests
{
    class Program
    {
        static Machine _machine = new Machine(null);
        static void Main()
        {
            ShowProgramHeader();
            bool run = true;

            ActionSelector<string> selector = new ActionSelector<string>(new Type[] { typeof(int), typeof(uint), typeof(ushort), typeof(string) });
            selector.RegisterActionsFromContainer(new MainActionsContainer(_machine, delegate()
            {
                run = false;
            }), true);
            string inputString;

            Console.Write("Loading microprogram.xml..");
            selector.ExecuteCommand("load micro microprogram.xml");
            Console.WriteLine("[done]\n");

            while (run)
            {
                Console.Write(">");
                inputString = Console.ReadLine();
                try
                {
                    Console.WriteLine(selector.ExecuteCommand(inputString));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }

                Console.WriteLine();
            }
        }

        private static void ShowProgramHeader()
        {
            AssemblyName assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            Version assemblyVersion = assemblyName.Version;
            Console.WriteLine("Snow - BasePC Emulator (v{0}). Copyright (C) 2008 cleancode.ru", assemblyVersion.ToString(3));
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY. For details see LICENSE.txt");
            Console.WriteLine("http://sourceforge.net/projects/snow-basepcemul/");
            Console.WriteLine();
        }

        public static void RAMCellChanged(DataCell cell)
        {
            Console.WriteLine("Cell 0x{0:X4} changed: 0x{1:X4}", (int)cell.Id, cell.Data);
        }
    }
}