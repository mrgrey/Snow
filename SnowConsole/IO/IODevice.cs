using System;
using System.Collections.Generic;
using System.Text;
using Cleancode.Snow.Memory;

namespace Cleancode.Snow.IO
{
    class IODevice
    {
        private Core.Machine _machine;
        public bool _ready;
        public bool Ready
        {
            get { return _ready; }
            set
            {
                _ready = value;
                if (value)
                    _machine.State[5] = true;
            }
        }
        public readonly Register DataRegister;
        public ushort Data
        {
            get
            {
                return DataRegister.Data;
            }
            set
            {
                DataRegister.Data = value;
                Ready = true;
            }
        }

        public IODevice(Core.Machine machine)
        {
            _ready = false;
            DataRegister = new Register(8);
            _machine = machine;
        }
    }
}
