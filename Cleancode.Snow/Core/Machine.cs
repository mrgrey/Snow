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
        /// �������� ������ ������
        /// </summary>
        public readonly Memory.RAM MainMemory;

        /// <summary>
        /// ������ �����������
        /// </summary>
        public readonly Memory.RAM MicrocommandsMemory;

        /// <summary>
        /// ������� ������
        /// </summary>
        public Memory.Register Data
        {
            get { return Registers[RegisterTypes.Data]; }
        }
        /// <summary>
        /// ������� ������
        /// </summary>
        public Memory.Register Command
        {
            get { return Registers[RegisterTypes.Command]; }
        }
        /// <summary>
        /// ������� ������
        /// </summary>
        public Memory.Register CommandCounter
        {
            get { return Registers[RegisterTypes.CommandCounter]; }
        }
        /// <summary>
        /// ����������
        /// </summary>
        public Memory.Register Accumulator
        {
            get { return Registers[RegisterTypes.Accumulator]; }
        }
        /// <summary>
        /// ������� ���������
        /// </summary>
        public Memory.Register State
        {
            get { return Registers[RegisterTypes.State]; }
        }
        /// <summary>
        /// ������������ �������
        /// </summary>
        public Memory.Register Keyboard
        {
            get { return Registers[RegisterTypes.Keyboard]; }
        }
        /// <summary>
        /// ������� ������
        /// </summary>
        public Memory.Register Address
        {
            get { return Registers[RegisterTypes.Address]; }
        }
        /// <summary>
        /// ������� ��������
        /// </summary>
        public Memory.Register Carry
        {
            get { return Registers[RegisterTypes.Carry]; }
        }

        /// <summary>
        /// ����� ���� ���
        /// </summary>
        public Memory.Register AluLeftInput
        {
            get { return Registers[RegisterTypes.AluLeftInput]; }
        }
        /// <summary>
        /// ������ ���� ���
        /// </summary>
        public Memory.Register AluRightInput
        {
            get { return Registers[RegisterTypes.AluRightInput]; }
        }
        /// <summary>
        /// �������� ������� (����� ���)
        /// </summary>
        public Memory.Register BufferRegister
        {
            get { return Registers[RegisterTypes.BufferRegister]; }
        }
        /// <summary>
        /// �������� ������� �������� (����� ���)
        /// </summary>
        public Memory.Register CarryBufferRegister
        {
            get { return Registers[RegisterTypes.CarryBufferRegister]; }
        }

        /// <summary>
        /// ������� �����������
        /// </summary>
        public Memory.Register MicrocommandsCounter
        {
            get { return Registers[RegisterTypes.MicrocommandsCounter]; }
        }
        /// <summary>
        /// ������� ������������
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
        /// ����������� �� ���������
        /// </summary>
        protected Machine()
        { }

        delegate void BinaryAction(RegisterTypes type, byte length);


        /// <summary>
        /// �����������, ��������� ��������� ����������� ���������� ��������� ������ �������� ������
        /// </summary>
        /// <param name="memoryCellsChangedHandler">����������� ���������� ����������� ������ �������� ������</param>
        public Machine(Memory.DataCellEventHandler memoryCellsChangedHandler)
        {

            /* �������������� �������� ������ (2� �����) � ��������� ��� ���� ����� 
             * ����� ����������� ���������� ���������
             * */
            MainMemory = new Memory.RAM(2048);
            MainMemory.CellChangedHandler = memoryCellsChangedHandler;
            // �������������� ������ �����������
            MicrocommandsMemory = new Cleancode.Snow.Memory.RAM(256);


            var registersList = new Register[]{
                // �������������� ��������, �������� � �������� ID ������� ������������ RegisterTypes
                new Memory.Register(16, RegisterTypes.Data), 
                new Memory.Register(16, RegisterTypes.Command),
                new Memory.Register(11, RegisterTypes.CommandCounter),
                new Memory.Register(16, RegisterTypes.Accumulator),
                new Memory.Register(13, RegisterTypes.State),
                new Memory.Register(16, RegisterTypes.Keyboard),
                new Memory.Register(12, RegisterTypes.Address),
                new Memory.Register(1, RegisterTypes.Carry),

                // �������������� ���������� ��������
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

            /* ������������� ����, ������������ ��� ����������� ����������� ��������� (GOTO)
             * ��� �������� �� ������ �������� �� ��� ����� ��������
             * */
            State.Data |= 2;

            

            MicrocommandsCounter.Data = 1;
        }

        /// <summary>
        /// ��������� ������������, �� ������� ��������� ������� �����������
        /// </summary>
        public void ProcessCurrentMicrocommand()
        {
            //�������� ������������, �� ������� ��������� ������� �����������, �� �������� � ������� �� ��������� (�������� ������ �� ����)
            Microcommands.MicroCommand currentMicrocommand = Microcommands.MicroCommand.Create(MicrocommandsMemory[MicrocommandsCounter.Data]);
            MicrocommandsCounter.Data++;
            //� ���� ������������ ��������������, ������� ����� "�� ����������" ����� ����������
            unchecked
            {
                //�������� ��������� � ����������� �� ���� ������������
                switch (currentMicrocommand.Type)
                {
                    //������������ ������������ 0
                    case Microcommands.MicroCommandType.Operational0:
                        {
                            //�������� ��������� ������������ � ���������������� ����
                            Microcommands.OperationalMicrocommand0 omc0 = (currentMicrocommand as Microcommands.OperationalMicrocommand0);
                            //���� ������� �������� ������������ ������ �������� ������������, �� ���������� ������ ��
                            if (omc0.RotateAccumulator != Cleancode.Snow.Microcommands.RotateAccumulatorMode.None)
                            {
                                if (omc0.RotateAccumulator == Microcommands.RotateAccumulatorMode.Left)
                                {
                                    /* 1) ���������� � �������� ������� �������� ������� ��� ������������
                                     * 2) �������� ���������� ������������ "�����" � ���������� � �������� ������� (� ������� �������� ����)
                                     * 3) ���������� � ������� ��� ��������� �������� �������� �������� �������� */
                                    CarryBufferRegister.Data = (ushort)(((Accumulator.Data & 0x8000) != 0) ? 1 : 0);
                                    BufferRegister.Data = (ushort)(Accumulator.Data << 1);
                                    BufferRegister.Data |= (ushort)(Carry.Data);
                                }
                                else
                                {
                                    /* 1) ���������� � �������� ������� �������� ������� ��� ������������
                                     * 2) �������� ���������� ����������� "������" � ���������� � �������� ������� (� ������� �������� ����)
                                     * 3) ���������� � ������� ��� ��������� �������� �������� �������� �������� */
                                    CarryBufferRegister.Data = (ushort)(Accumulator.Data & 0x0001);
                                    BufferRegister.Data = (ushort)(Accumulator.Data >> 1);
                                    BufferRegister.Data |= (ushort)(Carry.Data != 0 ? 0x8000 : 0);
                                }
                            }
                            //��������� �������� �� �������� ��������� ������������ ������
                            else
                            {
                                //AluLeftInput ���������� ����� �������� ������ �� ����� ���� ���
                                switch (omc0.AluLeftInput)
                                {
                                    //�����������
                                    case ALU.AluLeftInputSource.AccumulatorRegister:
                                        AluLeftInput.Data = Accumulator.Data;
                                        break;
                                    //������������ �������
                                    case ALU.AluLeftInputSource.KeyboardRegister:
                                        AluLeftInput.Data = Keyboard.Data;
                                        break;
                                    //������� ���������
                                    case ALU.AluLeftInputSource.StatesRegister:
                                        AluLeftInput.Data = State.Data;
                                        break;
                                    //����
                                    case ALU.AluLeftInputSource.Zero:
                                        AluLeftInput.Data = 0;
                                        break;
                                }
                                //AlueRightInput ���������� ����� �������� ������ �� ������ ���� ���
                                switch (omc0.AluRightInput)
                                {
                                    //������� ������
                                    case ALU.AluRightInputSource.CommandRegister:
                                        AluRightInput.Data = Command.Data;
                                        break;
                                    //������� ������
                                    case ALU.AluRightInputSource.CommandsCounterRegister:
                                        AluRightInput.Data = CommandCounter.Data;
                                        break;
                                    //������� ������
                                    case ALU.AluRightInputSource.DataRegister:
                                        AluRightInput.Data = Data.Data;
                                        break;
                                    //����
                                    case ALU.AluRightInputSource.Zero:
                                        AluRightInput.Data = 0;
                                        break;
                                }

                                //���� ���������� ����������� �������� ���������� �� ���� �� ������ ���
                                if (omc0.AluInputInvert != ALU.AluInputInvertMode.None)
                                    if (omc0.AluInputInvert == ALU.AluInputInvertMode.Left)
                                        AluLeftInput.Data = (ushort)(~AluLeftInput.Data);
                                    else
                                        AluRightInput.Data = (ushort)(~AluRightInput.Data);

                                //��������� ��������� ��������
                                if (omc0.AluOperation == ALU.AluOperationType.LogicalAnd)
                                    BufferRegister.Data = (ushort)(AluLeftInput.Data & AluRightInput.Data);
                                else
                                {
                                    /* 1) ���������� ��������, �������� �� �����, �������� ������������ (��� ����� ��������� ���������� �� ��������� ���������� �������� �������)
                                     * 2) ���� �������� - ����� ������ + 1, �� ����������� ��������� �� �������
                                     * 3) ���������� ��������� � �������� ������� (����� ���, ���������� �������)
                                     * 4) ���������� ��� �������� � �������� ������� ��������
                                     * */
                                    uint result = (uint)(AluLeftInput.Data + AluRightInput.Data);
                                    if (omc0.AluOperation == ALU.AluOperationType.SummAndOne)
                                        result += 1;

                                    BufferRegister.Data = (ushort)(result);
                                    CarryBufferRegister.Data = (ushort)(((result & 0x10000) != 0) ? 1 : 0);
                                }


                                /* ��������� ������������� �������� ������ ������� � �������
                                 * ������� ������ <=/=> ������ �������� ������
                                 * � ������� ������ � �������� ������ ������ ������������ ������ 11 ����� �������� ������,
                                 * ��� ��� ������� ��� ��������� �� ��� ���������
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
                    // ������������ ������������ 1
                    case Microcommands.MicroCommandType.Operational1:
                        {
                            //�������� ������������, �� ������� ��������� ������� �����������, �� �������� � ������� �� ��������� (�������� ������ �� ����)
                            Microcommands.OperationalMicrocommand1 omc1 = (currentMicrocommand as Microcommands.OperationalMicrocommand1);
                            // AluOutputOperation ���������� �������� �������� ��������� �������� ���
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

                            //CarryRegisterValueOperation ���������� �������� ��� ��������� ��������
                            switch (omc1.CarryRegisterValueOperation)
                            {
                                case Microcommands.CarryRegisterValueOperationType.Clear:
                                    Carry.Data = 0;
                                    break;
                                case Microcommands.CarryRegisterValueOperationType.Set:
                                    Carry.Data = 1;
                                    break;
                                //�������� �������� ��������� �������� �������� ���
                                case Microcommands.CarryRegisterValueOperationType.SetByBufferRegister:
                                    Carry.Data = CarryBufferRegister.Data;
                                    break;
                            }

                            //���� ����������, ������������� �������� ����� N(������������� �����) �������� ��������� �� ����� ������������
                            if ((omc1.NZRegistersValueOperation & Microcommands.NZRegistersValueOperationType.SetN) != 0)
                                State[2] = Accumulator[15];
                            //���� ����������, ������������� �������� ����� Z(����) �������� ��������� �� �������� ������������
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


                            //������������ �������� ����� "������" �������� ��������� �������������� ����������� �������
                            State[7] = !omc1.Halt;
                        }
                        break;
                    //����������� ������������
                    case Microcommands.MicroCommandType.Control:
                        {
                            //�������� ������������, �� ������� ��������� ������� �����������, �� �������� � ������� �� ��������� (�������� ������ �� ����)
                            Microcommands.ControlMicroCommand cmc = (currentMicrocommand as Microcommands.ControlMicroCommand);
                            Memory.DataCell currentRegister = null;

                            //�������� ����������� ������� �������� ArgumentSource
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

                            //���������� �������� ������������ �������� � �������� ������� ��� (�������� ���������� ���������� ������� �� ����������)
                            BufferRegister.Data = currentRegister.Data;

                            //���� ����������� ��� ����� ���������� ��������, �� ������ �������� �������� ����������� �� ��������
                            if (currentRegister[cmc.TestBitIndex] == cmc.Condition)
                                MicrocommandsCounter.Data = cmc.JumpAddress;
                        }
                        break;
                }
            }

        }

        /// <summary>
        /// ��������� ������ �� ���������� � ��������� ������������ �� ��� ���, ���� � �������� ��������� ������ ���� ������
        /// </summary>
        public void Run()
        {
            //���������, ��� ������ ��������� � ��������� ������
            State[7] = true;
            //���� ���� ������ �� ������� ��������� ������������
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