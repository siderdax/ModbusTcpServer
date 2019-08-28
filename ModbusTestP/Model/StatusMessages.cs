using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTestP.Model
{
    public static class StatusMessages
    {
        public const string CONNECT_MSG_1 = "Connect TCP, IP=";
        public const string CONNECT_MSG_2 = ", Port=";
        public const string ERROR_HEADER = "Error: ";
        public const string SUCCESS = "Success!";
        public const string DISCONNECTED = "Disconnected";
        public const string DISCONNECTION_ERR = "Disconnection error: ";
        public const string SETTINGS_SAVED = "Settings saved";
        public const string WRITE_HOLDING_MSG_1 = "Written Holding Register, Address:";
        public const string WRITE_HOLDING_MSG_2 = ", Register: ";
        public const string WRITE_INPUT_MSG_1 = "Written Input Register, Address:";
        public const string WRITE_INPUT_MSG_2 = ", Register: ";
        public const string INVALID_ADDRESS = "Invalid address";
        public const string HOLDING_CHANGED_MSG_1 = "Holding register changed,\n register: ";
        public const string HOLDING_CHANGED_MSG_2 = ", Number of registers: ";
    }
}
