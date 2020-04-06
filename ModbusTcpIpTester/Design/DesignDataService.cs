using System;
using System.Data;
using System.Net;
using ModbusTestP.Model;

namespace ModbusTestP.Design
{
    public class DesignDataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to create design time data
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            var item = new DataItem("Modbus Tester [design]", addr[1].ToString(), "502");

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