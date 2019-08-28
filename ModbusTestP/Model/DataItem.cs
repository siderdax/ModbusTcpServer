namespace ModbusTestP.Model
{
    public class DataItem
    {
        public string StatusText
        {
            get;
            private set;
        }

        public string Ip
        {
            get;
            private set;
        }

        public string Port
        {
            get;
            private set;
        }

        public ushort DefaultStartReadAddr
        {
            get;
            private set;
        }

        public ushort DefaultReadAddrLength
        {
            get;
            private set;
        }

        public ushort DefaultWriteHoldingValue
        {
            get;
            private set;
        }

        public ushort DefaultWriteHoldingAddr
        {
            get;
            private set;
        }

        public ushort DefaultWriteInputValue
        {
            get;
            private set;
        }

        public ushort DefaultWriteInputAddr
        {
            get;
            private set;
        }

        public byte DefaultSlaveId
        {
            get;
            private set;
        }

        public ModbusDataTypes[] ModbusReadTypes
        {
            get;
            private set;
        }

        public ModbusDataTypes[] ModbusWriteTypes
        {
            get;
            private set;
        }

        public DataItem(string statusText, string ip, string port)
        {
            StatusText = statusText;
            Ip = ip;
            Port = port;
        }

        public void SetModbusReadTypes(ModbusDataTypes[] mdt)
        {
            ModbusReadTypes = mdt;
        }

        public void SetModbusWriteTypes(ModbusDataTypes[] mdt)
        {
            ModbusWriteTypes = mdt;
        }

        /// <summary>Set default values</summary>
        /// <param name="sra">Default starting Read address value</param>
        /// <param name="srl">Default Read address length</param>
        /// <param name="wha">Default Holding register address</param>
        /// <param name="whv">Default Holding register value</param>
        /// <param name="wia">Default Input register address</param>
        /// <param name="wiv">Default Input register value</param>
        /// <param name="slvid">Slave id</param>
        public void SetDefaults(ushort sra, ushort srl, ushort wha, ushort whv,
            ushort wia, ushort wiv, byte slvid)
        {
            DefaultStartReadAddr = sra;
            DefaultReadAddrLength = srl;
            DefaultWriteHoldingAddr = wha;
            DefaultWriteHoldingValue = whv;
            DefaultWriteInputAddr = wia;
            DefaultWriteInputValue = wiv;
            DefaultSlaveId = slvid;
        }
    }
}