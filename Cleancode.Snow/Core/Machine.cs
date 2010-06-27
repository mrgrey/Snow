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
using System.Linq;
using System.Text;
using Cleancode.Snow;
using Cleancode.Snow.Memory;


namespace Cleancode.Snow.Core
{
    public class Machine
    {
        /// <summary>
        /// Основная память машины
        /// </summary>
        public readonly Memory.RAM MainMemory;

        /// <summary>
        /// Память микрокоманд
        /// </summary>
        public readonly Memory.RAM MicrocommandsMemory;

        /// <summary>
        /// Регистр данных
        /// </summary>
        public Memory.Register Data
        {
            get { return Registers[RegisterTypes.Data]; }
        }
        /// <summary>
        /// Регистр команд
        /// </summary>
        public Memory.Register Command
        {
            get { return Registers[RegisterTypes.Command]; }
        }
        /// <summary>
        /// Счетчик команд
        /// </summary>
        public Memory.Register CommandCounter
        {
            get { return Registers[RegisterTypes.CommandCounter]; }
        }
        /// <summary>
        /// Аккмулятор
        /// </summary>
        public Memory.Register Accumulator
        {
            get { return Registers[RegisterTypes.Accumulator]; }
        }
        /// <summary>
        /// Регистр состояний
        /// </summary>
        public Memory.Register State
        {
            get { return Registers[RegisterTypes.State]; }
        }
        /// <summary>
        /// Клавиатурный регистр
        /// </summary>
        public Memory.Register Keyboard
        {
            get { return Registers[RegisterTypes.Keyboard]; }
        }
        /// <summary>
        /// Регистр адреса
        /// </summary>
        public Memory.Register Address
        {
            get { return Registers[RegisterTypes.Address]; }
        }
        /// <summary>
        /// Регистр переноса
        /// </summary>
        public Memory.Register Carry
        {
            get { return Registers[RegisterTypes.Carry]; }
        }

        /// <summary>
        /// Левый вход АЛУ
        /// </summary>
        public Memory.Register AluLeftInput
        {
            get { return Registers[RegisterTypes.AluLeftInput]; }
        }
        /// <summary>
        /// Правый вход АЛУ
        /// </summary>
        public Memory.Register AluRightInput
        {
            get { return Registers[RegisterTypes.AluRightInput]; }
        }
        /// <summary>
        /// Буферный регистр (выход АЛУ)
        /// </summary>
        public Memory.Register BufferRegister
        {
            get { return Registers[RegisterTypes.BufferRegister]; }
        }
        /// <summary>
        /// Буферный регистр переноса (выход АЛУ)
        /// </summary>
        public Memory.Register CarryBufferRegister
        {
            get { return Registers[RegisterTypes.CarryBufferRegister]; }
        }

        /// <summary>
        /// Счетчик микрокоманд
        /// </summary>
        public Memory.Register MicrocommandsCounter
        {
            get { return Registers[RegisterTypes.MicrocommandsCounter]; }
        }
        /// <summary>
        /// Текущая микрокоманда
        /// </summary>
        public Memory.Register Microcommand
        {
            get { return Registers[RegisterTypes.Microcommand]; }
        }

        private Dictionary<RegisterTypes, Memory.Register> registers;

        public IDictionary<RegisterTypes, Memory.Register> Registers
        {
            get
            {
                return new Cleancode.Snow.Misc.ReadOnlyDictionary<RegisterTypes, Memory.Register>(registers);
            }
        }

        
        public readonly IO.IODevice[] IODevices;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        protected Machine()
        { }

        delegate void BinaryAction(RegisterTypes type, byte length);


        /// <summary>
        /// Конструктор, позволяет указывать стандартный обработчик изменения ячейки основной памяти
        /// </summary>
        /// <param name="memoryCellsChangedHandler">Стандартный обработчик измененения ячейки основной памяти</param>
        public Machine(Memory.DataCellEventHandler memoryCellsChangedHandler)
        {

            /* Инициализируем основную память (2К ячеек) и назначаем для всех ячеек 
             * общий стандартный обработчик изменения
             * */
            MainMemory = new Memory.RAM(2048);
            MainMemory.CellChangedHandler = memoryCellsChangedHandler;
            // Инициализируем память микрокоманд
            MicrocommandsMemory = new Cleancode.Snow.Memory.RAM(256);


            var registersList = new Register[]{
                // Инициализируем регистры, указывая в качестве ID элемент перечисления RegisterTypes
                new Memory.Register(16, RegisterTypes.Data), 
                new Memory.Register(16, RegisterTypes.Command),
                new Memory.Register(11, RegisterTypes.CommandCounter),
                new Memory.Register(16, RegisterTypes.Accumulator),
                new Memory.Register(13, RegisterTypes.State),
                new Memory.Register(16, RegisterTypes.Keyboard),
                new Memory.Register(12, RegisterTypes.Address),
                new Memory.Register(1, RegisterTypes.Carry),

                // Инициализируем внутренние регистры
                new Memory.Register(16, RegisterTypes.AluLeftInput),
                new Memory.Register(16, RegisterTypes.AluRightInput),
                new Memory.Register(16, RegisterTypes.BufferRegister),
                new Memory.Register(16, RegisterTypes.CarryBufferRegister),
                new Memory.Register(8, RegisterTypes.MicrocommandsCounter),
                new Memory.Register(16, RegisterTypes.Microcommand)
            }.ToList();

            this.registers = new Dictionary<RegisterTypes, Cleancode.Snow.Memory.Register>();

            registersList.ForEach(
                register => this.registers.Add((RegisterTypes)register.Id, register)
            );

            IODevices = new IO.IODevice[3];
            for (int e = 0; e < IODevices.Length; e++)
                IODevices[e] = new IO.IODevice(this);

            /* Устаналвиваем флаг, используемый для организации безусловных переходов (GOTO)
             * Его значение не должно меняться ни при каких условиях
             * */
            State.Data |= 2;

            

            MicrocommandsCounter.Data = 1;
        }

        /// <summary>
        /// Выполняет микрокоманду, на которую указывает счетчик микрокоманд
        /// </summary>
        public void ProcessCurrentMicrocommand()
        {
            //Выбираем микрокоманду, на которую указывает счетчик микрокоманд, из прошивки и создаем ее экземпляр (считывая данные из кода)
            Microcommands.MicroCommand currentMicrocommand = Microcommands.MicroCommand.Create(MicrocommandsMemory[MicrocommandsCounter.Data]);
            MicrocommandsCounter.Data++;
            //В коде используются преобразования, которые могут "не понравится" среде выполнения
            unchecked
            {
                //Выбираем поведение в зависимости от типа микрокоманды
                switch (currentMicrocommand.Type)
                {
                    //Операционная Микрокоманда 0
                    case Microcommands.MicroCommandType.Operational0:
                        {
                            //Приводим экземпляр микрокоманды к соответствующему типу
                            Microcommands.OperationalMicrocommand0 omc0 = (currentMicrocommand as Microcommands.OperationalMicrocommand0);
                            //Если указана операция циклического сдвига значения аккумулятора, то занимаемся только ей
                            if (omc0.RotateAccumulator != Cleancode.Snow.Microcommands.RotateAccumulatorMode.None)
                            {
                                if (omc0.RotateAccumulator == Microcommands.RotateAccumulatorMode.Left)
                                {
                                    /* 1) записываем в буферный регистр переноса старший бит аккумулятора
                                     * 2) сдвигаем содержимое аккумулятора "влево" и записываем в буферный регистр (с потерей старшего бита)
                                     * 3) записываем в младший бит буферного регистра значение регистра переноса */
                                    CarryBufferRegister.Data = (ushort)(((Accumulator.Data & 0x8000) != 0) ? 1 : 0);
                                    BufferRegister.Data = (ushort)(Accumulator.Data << 1);
                                    BufferRegister.Data |= (ushort)(Carry.Data);
                                }
                                else
                                {
                                    /* 1) записываем в буферный регистр переноса младший бит аккумулятора
                                     * 2) сдвигаем содержимое аккумулятор "вправо" и записываем в буферный регистр (с потерей младшего бита)
                                     * 3) записываем в старший бит буферного регистра значение регистра переноса */
                                    CarryBufferRegister.Data = (ushort)(Accumulator.Data & 0x0001);
                                    BufferRegister.Data = (ushort)(Accumulator.Data >> 1);
                                    BufferRegister.Data |= (ushort)(Carry.Data != 0 ? 0x8000 : 0);
                                }
                            }
                            //Указанная операция не является операцией циклического сдвига
                            else
                            {
                                //AluLeftInput определяет какое значение подать на левый вход АЛУ
                                switch (omc0.AluLeftInput)
                                {
                                    //Аккумулятор
                                    case ALU.AluLeftInputSource.AccumulatorRegister:
                                        AluLeftInput.Data = Accumulator.Data;
                                        break;
                                    //Клавиатурный регистр
                                    case ALU.AluLeftInputSource.KeyboardRegister:
                                        AluLeftInput.Data = Keyboard.Data;
                                        break;
                                    //Регистр состояний
                                    case ALU.AluLeftInputSource.StatesRegister:
                                        AluLeftInput.Data = State.Data;
                                        break;
                                    //Ноль
                                    case ALU.AluLeftInputSource.Zero:
                                        AluLeftInput.Data = 0;
                                        break;
                                }
                                //AlueRightInput определяет какое значение подать на правый вход АЛУ
                                switch (omc0.AluRightInput)
                                {
                                    //Регистр команд
                                    case ALU.AluRightInputSource.CommandRegister:
                                        AluRightInput.Data = Command.Data;
                                        break;
                                    //Счетчик команд
                                    case ALU.AluRightInputSource.CommandsCounterRegister:
                                        AluRightInput.Data = CommandCounter.Data;
                                        break;
                                    //Регистр данных
                                    case ALU.AluRightInputSource.DataRegister:
                                        AluRightInput.Data = Data.Data;
                                        break;
                                    //Ноль
                                    case ALU.AluRightInputSource.Zero:
                                        AluRightInput.Data = 0;
                                        break;
                                }

                                //если необходимо инвертируем значение подаваемое на один из входов АЛУ
                                if (omc0.AluInputInvert != ALU.AluInputInvertMode.None)
                                    if (omc0.AluInputInvert == ALU.AluInputInvertMode.Left)
                                        AluLeftInput.Data = (ushort)(~AluLeftInput.Data);
                                    else
                                        AluRightInput.Data = (ushort)(~AluRightInput.Data);

                                //выполняем требуемую операцию
                                if (omc0.AluOperation == ALU.AluOperationType.LogicalAnd)
                                    BufferRegister.Data = (ushort)(AluLeftInput.Data & AluRightInput.Data);
                                else
                                {
                                    /* 1) складываем значения, поданные на входы, сохраняя переполнение (для этого результат записываем во временную переменную бОльшего размера)
                                     * 2) если операция - сумма входов + 1, то увеличиваем результат на единицу
                                     * 3) записываем результат в буферный регистр (теряя бит, содержащий перенос)
                                     * 4) записываем бит переноса в буферный регистр переноса
                                     * */
                                    uint result = (uint)(AluLeftInput.Data + AluRightInput.Data);
                                    if (omc0.AluOperation == ALU.AluOperationType.SummAndOne)
                                        result += 1;

                                    BufferRegister.Data = (ushort)(result);
                                    CarryBufferRegister.Data = (ushort)(((result & 0x10000) != 0) ? 1 : 0);
                                }


                                /* Выполняем одностороннюю операцию обмена данными с памятью
                                 * Регистр данных <=/=> Ячейка основной памяти
                                 * В опреции обмена в качестве адреса ячейки используются только 11 битов регистра адреса,
                                 * так как старший бит указывает на тип адресации
                                 * */
                                if (omc0.MemoryOperation != Microcommands.MemoryOperationType.None)
                                {
                                    if (omc0.MemoryOperation == Microcommands.MemoryOperationType.Read)
                                        Data.Data = MainMemory[Address.Data & 0x7ff];
                                    else
                                        MainMemory[Address.Data & 0x7ff] = Data.Data;
                                }
                            }
                        }
                        break;
                    // Операционная Микрокоманда 1
                    case Microcommands.MicroCommandType.Operational1:
                        {
                            //Выбираем микрокоманду, на которую указывает счетчик микрокоманд, из прошивки и создаем ее экземпляр (считывая данные из кода)
                            Microcommands.OperationalMicrocommand1 omc1 = (currentMicrocommand as Microcommands.OperationalMicrocommand1);
                            // AluOutputOperation определяет приемник значения буферного регистра АЛУ
                            switch (omc1.AluOutputOperation)
                            {
                                case ALU.AluOutputAcceptor.ToAccumulator:
                                    Accumulator.Data = BufferRegister.Data;
                                    break;
                                case ALU.AluOutputAcceptor.ToAddressRegister:
                                    Address.Data = BufferRegister.Data;
                                    break;
                                case ALU.AluOutputAcceptor.ToCommandCounter:
                                    CommandCounter.Data = BufferRegister.Data;
                                    break;
                                case ALU.AluOutputAcceptor.ToCommandRegister:
                                    Command.Data = BufferRegister.Data;
                                    break;
                                case ALU.AluOutputAcceptor.ToDataRegister:
                                    Data.Data = BufferRegister.Data;
                                    break;
                                case ALU.AluOutputAcceptor.ToAllWithoutCommandCounter:
                                    Data.Data = BufferRegister.Data;
                                    Accumulator.Data = BufferRegister.Data;
                                    Command.Data = BufferRegister.Data;
                                    Address.Data = BufferRegister.Data;
                                    break;
                            }

                            //CarryRegisterValueOperation определяет операцию над регистром переноса
                            switch (omc1.CarryRegisterValueOperation)
                            {
                                case Microcommands.CarryRegisterValueOperationType.Clear:
                                    Carry.Data = 0;
                                    break;
                                case Microcommands.CarryRegisterValueOperationType.Set:
                                    Carry.Data = 1;
                                    break;
                                //копируем значение буферного регистра переноса АЛУ
                                case Microcommands.CarryRegisterValueOperationType.SetByBufferRegister:
                                    Carry.Data = CarryBufferRegister.Data;
                                    break;
                            }

                            //Если необходимо, устанавливаем значение флага N(отрицательное число) регистра состояния по знаку аккумулятора
                            if ((omc1.NZRegistersValueOperation & Microcommands.NZRegistersValueOperationType.SetN) != 0)
                                State[2] = Accumulator[15];
                            //Если необходимо, устанавливаем значение флага Z(ноль) регистра состояния по значению аккумулятора
                            if ((omc1.NZRegistersValueOperation & Microcommands.NZRegistersValueOperationType.SetZ) != 0)
                                State[1] = Accumulator.Data == 0;

                            if (omc1.IOOperation != Cleancode.Snow.IO.IOOperations.None)
                                switch (omc1.IOOperation)
                                {
                                    case Cleancode.Snow.IO.IOOperations.DisableInterrupt:
                                        State[4] = false;
                                        break;
                                    case Cleancode.Snow.IO.IOOperations.EnableInterrupt:
                                        State[4] = true;
                                        State[5] = false;
                                        foreach (IO.IODevice currentIODevice in IODevices)
                                            State[5] |= currentIODevice.Ready;
                                        break;
                                    case Cleancode.Snow.IO.IOOperations.ClearIOFlags:
                                        for (int e = 0; e < IODevices.Length; e++)
                                            IODevices[e].Ready = false;
                                        break;
                                    case Cleancode.Snow.IO.IOOperations.ProcessIO:
                                        int index = (Command.Data & 0xFF) - 1;
                                        switch ((Command.Data >> 8) & 0xF)
                                        {
                                            case 0:
                                                IODevices[index].Ready = false;
                                                break;
                                            case 1:
                                                if (IODevices[index].Ready)
                                                    CommandCounter.Data++;
                                                break;
                                            case 2:
                                                Accumulator.Data = (ushort)((Accumulator.Data & 0xFF00) | IODevices[index].Data);
                                                break;
                                            case 3:
                                                IODevices[index].Data = Accumulator.Data;
                                                break;
                                        }
                                        break;
                                }


                            //Устаналиваем значение флага "работа" регистра состояний соответственно содержимому команды
                            State[7] = !omc1.Halt;
                        }
                        break;
                    //Управляющая Микрокоманда
                    case Microcommands.MicroCommandType.Control:
                        {
                            //Выбираем микрокоманду, на которую указывает счетчик микрокоманд, из прошивки и создаем ее экземпляр (считывая данные из кода)
                            Microcommands.ControlMicroCommand cmc = (currentMicrocommand as Microcommands.ControlMicroCommand);
                            Memory.DataCell currentRegister = null;

                            //выбираем проверяемый регистр согласно ArgumentSource
                            switch (cmc.ArgumentSource)
                            {
                                case Microcommands.ControlMCArgumentSource.Accumulator:
                                    currentRegister = Accumulator;
                                    break;
                                case Microcommands.ControlMCArgumentSource.CommandRegister:
                                    currentRegister = Command;
                                    break;
                                case Microcommands.ControlMCArgumentSource.DataRegister:
                                    currentRegister = Data;
                                    break;
                                case Microcommands.ControlMCArgumentSource.StatesRegister:
                                    currentRegister = State;
                                    break;
                            }

                            //записываем значение проверяемого регистра в буферный регистр АЛУ (эмуляция результата выполнения команды на БазовойЭВМ)
                            BufferRegister.Data = currentRegister.Data;

                            //если проверяемый бит равен указанному значению, то меняем значение счетчика микрокоманд на заданное
                            if (currentRegister[cmc.TestBitIndex] == cmc.Condition)
                                MicrocommandsCounter.Data = cmc.JumpAddress;
                        }
                        break;
                }
            }

        }

        /// <summary>
        /// Запускает машину на выполнение и выполняет микрокоманды до тех пор, пока в регистре состояний указан флаг работы
        /// </summary>
        public void Run()
        {
            //указываем, что машина переходит в состояние работы
            State[7] = true;
            //пока флаг работы не сброшен выполняем микрокоманды
            while (State[7])
                ProcessCurrentMicrocommand();
        }

        public void ProcessMicroCycle()
        {
            do{
                this.ProcessCurrentMicrocommand();
            }while(this.MicrocommandsCounter.Data != 1);
        }
    }
}