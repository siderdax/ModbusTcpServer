namespace ModbusTestP.Model
{
    public class ModbusDataValue
    {
        public ushort Address
        {
            get;
            private set;
        }

        public ushort Register
        {
            get;
            private set;
        }

        public bool Coil
        {
            get;
            private set;
        }

        public void SetAddress(ushort address)
        {
            Address = address;
        }

        public void SetRegister(ushort register)
        {
            Register = register;
        }

        public void SetCoil(bool coil)
        {
            Coil = coil;
        }

        public ModbusDataValue(ushort address, ushort register)
        {
            Address = address;
            Register = register;
        }

        public ModbusDataValue(ushort address, ushort register, bool coil) : this(address, register)
        {
            Address = address;
            Register = register;
            Coil = coil;
        }
    }
}