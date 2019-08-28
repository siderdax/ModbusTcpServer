using System;
using System.Net;

namespace ModbusTestP.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            var item = new DataItem("Modbus Tester", addr[1].ToString(), "502");

            ModbusDataTypes[] modbusReadTypeSet = {
                new ModbusDataTypes("Holding Register", ModbusDataTypes.RD_HOLDINGREG),
                new ModbusDataTypes("Input Register", ModbusDataTypes.RD_INPUTREG)
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