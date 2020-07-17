using System;
using ModbusTcpIp;

namespace ModbusTestP.Model
{
    public interface IDataService
    {
        ModbusTcp ModbusTCP { get; set; }
        ModbusIp ModbusIP { get; set; }

        void GetData(Action<DataItem, Exception> callback);
    }
}
