using System;
using ModbusTcpIp;
using ModbusTestP.Model;

namespace ModbusTestP.Design
{
    public class DesignDataService : IDataService
    {
        public ModbusTcp ModbusTCP { get; set; }
        public ModbusIp ModbusIP { get; set; }
        
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to create design time data
            var item = new DataItem("Modbus Tester", "127.0.0.1", "502");

            ModbusDataTypes[] modbusReadTypeSet = {
                new ModbusDataTypes("Holding Register", 0x03),
                new ModbusDataTypes("Input Register", 0x04)
            };
            item.SetModbusReadTypes(modbusReadTypeSet);

            ModbusDataTypes[] modbusWriteTypeSet = {
                new ModbusDataTypes("Input Register", 0xFF)
            };
            item.SetModbusWriteTypes(modbusWriteTypeSet);

            item.SetDefaults(1, 18, 1, 0, 1, 0, 1);
            
            callback(item, null);
        }
    }
}