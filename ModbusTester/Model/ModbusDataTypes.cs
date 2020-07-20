namespace ModbusTestP.Model
{
    public class ModbusDataTypes
    {
        public const int RD_HOLDINGREG = 0x03;
        public const int RD_INPUTREG = 0x04;
        public const int WR_HOLDINGREG_SNGL = 0x06;
        public const int WR_HOLDINGREG_MULT = 0x10;

        public string Name
        {
            get;
            private set;
        }

        public byte FunctionCode
        {
            get;
            private set;
        }

        public byte FunctionCodeMultiple
        {
            get;
            private set;
        }

        public ModbusDataTypes(string name, byte fc)
        {
            Name = name;
            FunctionCode = fc;
        }

        public ModbusDataTypes(string name, byte fcSingle, byte fcMulti) : this(name, fcSingle)
        {
            Name = name;
            FunctionCode = fcSingle;
            FunctionCodeMultiple = fcMulti;
        }
    }
}