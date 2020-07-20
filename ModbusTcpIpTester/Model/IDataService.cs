using System;
using ModbusTcpIp;

namespace ModbusTestP.Model
{
    public interface IDataService
    {
        ModbusTcp ModbusTCP { get; set; }
        ModbusIp ModbusIP { get; set; }
        ModbusSlave ModbusSlave { get; set; }
        ModbusMaster ModbusMaster { get; set; }

        void GetData(Action<DataItem, Exception> callback);
    }
}
