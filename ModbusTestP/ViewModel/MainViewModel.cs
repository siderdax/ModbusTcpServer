using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModbusTcpIp;
using ModbusTestP.Model;

namespace ModbusTestP.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private const int TAB_SERVER = 0;
        private const int TAB_CLIENT = 1;

        private readonly IDataService _dataService;
        private ModbusTcp modbusTcp;
        private ModbusIp modbusIp;

        private DispatcherTimer timer = null;
        private EventHandler TcpEventHandler, IpEventHandler;

        #region Base properties

        private string _statusText = string.Empty;
        private string _port = string.Empty;
        private string _ip = string.Empty;
        private byte _slaveId;
        private ushort _modStartReadAddr;
        private ushort _modReadAddrLength;
        private ushort _savedModStartReadAddr;
        private ushort _savedModReadAddrLength;
        private ushort _modWriteHoldingAddr;
        private ushort _modWriteHoldingValue;
        private ushort _modWriteInputAddr;
        private ushort _modWriteInputValue;
        private bool _isDisconnected;
        private bool _isServerConnected;
        private bool _isClientConnected;

        /// Changes to that property's value raise the PropertyChanged event. 
        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                Set(ref _statusText, value);
            }
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { Set(ref _selectedTabIndex, value); }
        }

        public string Port
        {
            get
            {
                return _port;
            }
            set
            {
                Set(ref _port, value);
            }
        }

        public string Ip
        {
            get
            {
                return _ip;
            }
            set
            {
                Set(ref _ip, value);
            }
        }

        public byte SlaveId
        {
            get
            {
                return _slaveId;
            }
            set
            {
                Set(ref _slaveId, value);
            }
        }

        public ushort ModStartReadAddr
        {
            get
            {
                return _modStartReadAddr;
            }
            set
            {
                Set(ref _modStartReadAddr, value);
            }
        }

        public ushort ModReadAddrLength
        {
            get
            {
                return _modReadAddrLength;
            }
            set
            {
                Set(ref _modReadAddrLength, value);
            }
        }

        public ushort SavedModStartReadAddr
        {
            get
            {
                return _savedModStartReadAddr;
            }
            set
            {
                Set(ref _savedModStartReadAddr, value);
            }
        }

        public ushort SavedModReadAddrLength
        {
            get
            {
                return _savedModReadAddrLength;
            }
            set
            {
                Set(ref _savedModReadAddrLength, value);
            }
        }

        public ushort ModWriteHoldingAddr
        {
            get
            {
                return _modWriteHoldingAddr;
            }
            set
            {
                Set(ref _modWriteHoldingAddr, value);
            }
        }

        public ushort ModWriteHoldingValue
        {
            get
            {
                return _modWriteHoldingValue;
            }
            set
            {
                Set(ref _modWriteHoldingValue, value);
            }
        }

        public ushort ModWriteInputAddr
        {
            get
            {
                return _modWriteInputAddr;
            }
            set
            {
                Set(ref _modWriteInputAddr, value);
            }
        }

        public ushort ModWriteInputValue
        {
            get
            {
                return _modWriteInputValue;
            }
            set
            {
                Set(ref _modWriteInputValue, value);
            }
        }

        public bool IsDisconnected
        {
            get
            {
                return _isDisconnected;
            }
            set
            {
                Set(ref _isDisconnected, value);
            }
        }

        public bool IsServerConnected
        {
            get
            {
                return _isServerConnected;
            }
            set
            {
                Set(ref _isServerConnected, value);
            }
        }

        public bool IsClientConnected
        {
            get
            {
                return _isClientConnected;
            }
            set
            {
                Set(ref _isClientConnected, value);
            }
        }

        #endregion

        /// ListView status properties
        #region ListView properties

        private ObservableCollection<ModbusDataValue> _modReadHoldingList;
        public ObservableCollection<ModbusDataValue> ModReadHoldingList
        {
            get
            {
                return _modReadHoldingList;
            }
            set
            {
                Set(ref _modReadHoldingList, value);
            }
        }

        private ObservableCollection<ModbusDataValue> _modReadInputList;
        public ObservableCollection<ModbusDataValue> ModReadInputList
        {
            get
            {
                return _modReadInputList;
            }
            set
            {
                Set(ref _modReadInputList, value);
            }
        }

        #endregion

        /// Button commands
        #region Button command properties

        public ICommand ConnectCmd
        {
            get;
            private set;
        }

        public ICommand DisconnectCmd
        {
            get;
            private set;
        }

        public ICommand SaveCmd
        {
            get;
            private set;
        }

        public ICommand WriteHoldingCmd
        {
            get;
            private set;
        }

        public ICommand WriteInputCmd
        {
            get;
            private set;
        }

        #endregion

        /// Button command Methods
        #region Button commands

        public void ConnectCmdMethod()
        {
            switch (SelectedTabIndex)
            {
                case TAB_SERVER:
                    ConnectServerCmdMethod();
                    break;
                case TAB_CLIENT:
                    ConnectClientCmdMethod();
                    break;
                default:
                    break;
            }
        }

        public void ConnectServerCmdMethod()
        {
            StatusText = StatusMessages.CONNECT_MSG_1 + Ip +
                StatusMessages.CONNECT_MSG_2 + Port + "\n";

            modbusTcp = new ModbusTcp(null, Port);
            try
            {
                modbusTcp.StartServer();
                modbusTcp.StartTcpSlave(SlaveId);
                IsDisconnected = false;
                IsServerConnected = true;
                timer.Tick += TcpEventHandler;
                timer.Start();
            }
            catch (Exception e)
            {
                StatusText += StatusMessages.ERROR_HEADER + e.Message;
            }

            StatusText += StatusMessages.SUCCESS;
        }

        public void ConnectClientCmdMethod()
        {
            StatusText = StatusMessages.CONNECT_MSG_1 + Ip +
            StatusMessages.CONNECT_MSG_2 + Port + "\n";

            modbusIp = new ModbusIp(Ip, Port);
            try
            {
                modbusIp.StartClient();
                modbusIp.StartIpMaster();
                IsDisconnected = false;
                IsClientConnected = true;
                timer.Tick += IpEventHandler;
                timer.Start();
                StatusText += StatusMessages.SUCCESS;
            }
            catch (Exception e)
            {
                StatusText += StatusMessages.ERROR_HEADER + e.Message;
            }
        }

        public void DisconnectCmdMethod()
        {
            switch (SelectedTabIndex)
            {
                case TAB_SERVER:
                    DisconnectServerCmdMethod();
                    break;
                case TAB_CLIENT:
                    DisconnectClientCmdMethod();
                    break;
                default:
                    break;
            }
        }

        public void DisconnectServerCmdMethod()
        {
            try
            {
                timer.Stop();
                timer.Tick -= TcpEventHandler;
                modbusTcp.StopTcpSlave(SlaveId);
                modbusTcp.StopServer();
                ModReadHoldingList.Clear();
                ModReadInputList.Clear();
                StatusText = StatusMessages.DISCONNECTED;
                IsDisconnected = true;
                IsServerConnected = false;
            }
            catch (Exception e)
            {
                StatusText = StatusMessages.DISCONNECTION_ERR + e.Message;
            }
        }

        public void DisconnectClientCmdMethod()
        {
            try
            {
                timer.Stop();
                timer.Tick -= IpEventHandler;
                modbusIp.StopClient();
                modbusIp.StopIpMaster();
                ModReadHoldingList.Clear();
                ModReadInputList.Clear();
                StatusText = StatusMessages.DISCONNECTED;
                IsDisconnected = true;
                IsServerConnected = false;
            }
            catch (Exception e)
            {
                StatusText = StatusMessages.DISCONNECTION_ERR + e.Message;
            }
        }

        public void SaveCmdMethod()
        {
            ModStartReadAddr = ModStartReadAddr > 0 ? ModStartReadAddr : (ushort)1;
            ModReadAddrLength = ModReadAddrLength > 0 ? ModReadAddrLength : (ushort)1;
            if (ModStartReadAddr + ModReadAddrLength > 0x10000)
            {
                ModReadAddrLength = (ushort)(0x10000 - ModStartReadAddr);
            }
            SavedModReadAddrLength = ModReadAddrLength;
            SavedModStartReadAddr = ModStartReadAddr;

            ModReadInputList.Clear();
            ModReadHoldingList.Clear();

            StatusText = StatusMessages.SETTINGS_SAVED;
        }

        public void WriteHoldingCmdMethod()
        {
            switch(SelectedTabIndex)
            {
                case TAB_SERVER:
                    WriteHoldingFromServerCmdMethod();
                    break;
                case TAB_CLIENT:
                    WriteHoldingFromClientCmdMethod();
                    break;
                default:
                    break;
            }
        }

        public void WriteHoldingFromServerCmdMethod()
        {
            if (ModWriteHoldingAddr <= 0xFFFF)
            {
                modbusTcp.WriteTcpSlaveHoldingRegister(SlaveId, ModWriteHoldingAddr, ModWriteHoldingValue);
                UpdateTcpReadList(ModbusDataTypes.RD_HOLDINGREG);
                StatusText = StatusMessages.WRITE_HOLDING_MSG_1 + ModWriteHoldingAddr +
                    StatusMessages.WRITE_HOLDING_MSG_2 + ModWriteHoldingValue;
            }
            else
            {
                StatusText = StatusMessages.INVALID_ADDRESS;
            }
        }

        public void WriteHoldingFromClientCmdMethod()
        {
            try
            {
                if (ModWriteHoldingAddr <= 0xFFFF)
                {
                    modbusIp.WriteMasterHoldingRegister(
                        SlaveId, ModWriteHoldingAddr, ModWriteHoldingValue);
                    StatusText = StatusMessages.WRITE_HOLDING_MSG_1 + ModWriteHoldingAddr +
                        StatusMessages.WRITE_HOLDING_MSG_2 + ModWriteHoldingValue;
                }
                else
                {
                    StatusText = StatusMessages.INVALID_ADDRESS;
                }
            }
            catch (Exception e)
            {
                StatusText = StatusMessages.ERROR_HEADER + e.Message;
            }
        }

        public void WriteInputCmdMethod()
        {
            if (ModWriteInputAddr <= 0xFFFF)
            {
                modbusTcp.WriteTcpSlaveInputRegister(SlaveId, ModWriteInputAddr, ModWriteInputValue);
                UpdateTcpReadList(ModbusDataTypes.RD_INPUTREG);
                StatusText = StatusMessages.WRITE_INPUT_MSG_1 + ModWriteInputAddr +
                    StatusMessages.WRITE_INPUT_MSG_2 + ModWriteInputValue;
            }
            else
            {
                StatusText = StatusMessages.INVALID_ADDRESS;
            }
        }

        #endregion

        /// Other Functions

        private void UpdateTcpReadList(byte fc)
        {
            try
            {
                switch (fc)
                {
                    case ModbusDataTypes.RD_INPUTREG:
                        for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                        {
                            int address = idx + SavedModStartReadAddr;
                            ushort inputRegister = modbusTcp.ReadTcpSlaveInputRegister(
                                SlaveId, (ushort)address);

                            UpdateReadItem(ModReadInputList, idx, (ushort)address, inputRegister);
                        }
                        break;
                    case ModbusDataTypes.RD_HOLDINGREG:
                        for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                        {
                            int address = idx + SavedModStartReadAddr;
                            ushort holdingRegister = modbusTcp.ReadTcpSlaveHoldingRegister(
                                SlaveId, (ushort)address);

                            UpdateReadItem(ModReadHoldingList, idx, (ushort)address, holdingRegister);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                StatusText = e.Message;
            }
        }

        private void TcpTimerTick(object send, EventArgs e)
        {
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                UpdateTcpReadList(ModbusDataTypes.RD_INPUTREG);
                UpdateTcpReadList(ModbusDataTypes.RD_HOLDINGREG);
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => UpdateTcpReadList(ModbusDataTypes.RD_INPUTREG)));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => UpdateTcpReadList(ModbusDataTypes.RD_HOLDINGREG)));
            }
        }

        private void UpdateIpReadList(byte fc)
        {
            ushort[] data;
            try
            {
                switch (fc)
                {
                    case ModbusDataTypes.RD_INPUTREG:
                        data = modbusIp.ReadIpMasterInputRegisters(
                            SlaveId, SavedModStartReadAddr, SavedModReadAddrLength);
                        for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                        {
                            int address = idx + SavedModStartReadAddr;
                            UpdateReadItem(ModReadInputList, idx, (ushort)address, data[idx]);
                        }
                        break;
                    case ModbusDataTypes.RD_HOLDINGREG:
                        data = modbusIp.ReadIpMasterHoldingRegisters(
                            SlaveId, SavedModStartReadAddr, SavedModReadAddrLength);
                        for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                        {
                            int address = idx + SavedModStartReadAddr;
                            UpdateReadItem(ModReadHoldingList, idx, (ushort)address, data[idx]);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                StatusText = e.Message;
            }
        }

        private void IpTimerTick(object send, EventArgs e)
        {
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                UpdateIpReadList(ModbusDataTypes.RD_INPUTREG);
                UpdateIpReadList(ModbusDataTypes.RD_HOLDINGREG);
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => UpdateIpReadList(ModbusDataTypes.RD_INPUTREG)));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => UpdateIpReadList(ModbusDataTypes.RD_HOLDINGREG)));
            }
        }

        private void UpdateReadItem(ObservableCollection<ModbusDataValue> collection, int index, ushort address, ushort register)
        {
            if (collection.Count() <= index)
            {
                collection.Add(new ModbusDataValue(address, register));
            }
            else
            {
                if (collection[index].Address != address || collection[index].Register != register)
                {
                    collection.RemoveAt(index);
                    collection.Insert(index, new ModbusDataValue((ushort)address, register));
                }
            }

        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    StatusText = item.StatusText;
                    Ip = item.Ip;
                    Port = item.Port;
                    ModStartReadAddr = item.DefaultStartReadAddr;
                    ModReadAddrLength = item.DefaultReadAddrLength;
                    ModWriteHoldingAddr = item.DefaultWriteHoldingAddr;
                    ModWriteHoldingValue = item.DefaultWriteHoldingValue;
                    ModWriteInputAddr = item.DefaultWriteInputAddr;
                    ModWriteInputValue = item.DefaultWriteInputValue;
                    SlaveId = item.DefaultSlaveId;
                });

            ConnectCmd = new RelayCommand(ConnectCmdMethod);
            DisconnectCmd = new RelayCommand(DisconnectCmdMethod);
            SaveCmd = new RelayCommand(SaveCmdMethod);
            WriteHoldingCmd = new RelayCommand(WriteHoldingCmdMethod);
            WriteInputCmd = new RelayCommand(WriteInputCmdMethod);

            ModReadHoldingList = new ObservableCollection<ModbusDataValue>();
            ModReadInputList = new ObservableCollection<ModbusDataValue>();

            SavedModStartReadAddr = ModStartReadAddr;
            SavedModReadAddrLength = ModReadAddrLength;

            IsDisconnected = true;
            IsServerConnected = false;
            IsClientConnected = false;
            SelectedTabIndex = TAB_SERVER;

            // Timer
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            timer = dispatcherTimer;
            timer.Interval = TimeSpan.FromMilliseconds(300);
            TcpEventHandler = new EventHandler(TcpTimerTick);
            IpEventHandler = new EventHandler(IpTimerTick);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                modbusTcp.Dispose();
            }
        }

        ~MainViewModel()
        {
            Dispose(false);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}