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

        private DispatcherTimer timer = null;
        private EventHandler TcpEventHandler, IpEventHandler;

        #region Base properties

        private string _statusText = string.Empty;
        private string _port = string.Empty;
        private string _ip = string.Empty;
        private string _com = string.Empty;
        private string _baud = string.Empty;
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
            get => _statusText;
            set => Set(ref _statusText, value);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => Set(ref _selectedTabIndex, value);
        }

        public string Port
        {
            get => _port;
            set => Set(ref _port, value);
        }

        public string Ip
        {
            get => _ip;
            set => Set(ref _ip, value);
        }

        public string Com
        {
            get => _com;
            set => Set(ref _com, value);
        }

        public string Baud
        {
            get => _baud;
            set => Set(ref _baud, value);
        }

        public byte SlaveId
        {
            get => _slaveId;
            set => Set(ref _slaveId, value);
        }

        public ushort ModStartReadAddr
        {
            get => _modStartReadAddr;
            set => Set(ref _modStartReadAddr, value);
        }

        public ushort ModReadAddrLength
        {
            get => _modReadAddrLength;
            set => Set(ref _modReadAddrLength, value);
        }

        public ushort SavedModStartReadAddr
        {
            get => _savedModStartReadAddr;
            set => Set(ref _savedModStartReadAddr, value);
        }

        public ushort SavedModReadAddrLength
        {
            get => _savedModReadAddrLength;
            set => Set(ref _savedModReadAddrLength, value);
        }

        public ushort ModWriteHoldingAddr
        {
            get => _modWriteHoldingAddr;
            set => Set(ref _modWriteHoldingAddr, value);
        }

        public ushort ModWriteHoldingValue
        {
            get => _modWriteHoldingValue;
            set => Set(ref _modWriteHoldingValue, value);
        }

        public ushort ModWriteInputAddr
        {
            get => _modWriteInputAddr;
            set => Set(ref _modWriteInputAddr, value);
        }

        public ushort ModWriteInputValue
        {
            get => _modWriteInputValue;
            set => Set(ref _modWriteInputValue, value);
        }

        public bool IsDisconnected
        {
            get => _isDisconnected;
            set => Set(ref _isDisconnected, value);
        }

        public bool IsServerConnected
        {
            get => _isServerConnected;
            set => Set(ref _isServerConnected, value);
        }

        public bool IsClientConnected
        {
            get => _isClientConnected;
            set => Set(ref _isClientConnected, value);
        }

        #endregion

        /// ListView status properties
        #region ListView properties

        private ObservableCollection<ModbusDataValue> _modReadHoldingList;
        public ObservableCollection<ModbusDataValue> ModReadHoldingList
        {
            get => _modReadHoldingList;
            set => Set(ref _modReadHoldingList, value);
        }

        private ObservableCollection<ModbusDataValue> _modReadInputList;
        public ObservableCollection<ModbusDataValue> ModReadInputList
        {
            get => _modReadInputList;
            set => Set(ref _modReadInputList, value);
        }

        #endregion

        /// Button commands
        #region Button command properties

        public ICommand ConnectCmd { get; private set; }
        public ICommand DisconnectCmd { get; private set; }
        public ICommand SaveCmd { get; private set; }
        public ICommand WriteHoldingCmd { get; private set; }
        public ICommand WriteInputCmd { get; private set; }

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
            StatusText = StatusMessages.CONNECT_MSG_1 + Ip + StatusMessages.CONNECT_MSG_2 + Port + "\n";

            _dataService.ModbusTCP = new ModbusTcp(null, Port);
            try
            {
                _dataService.ModbusTCP.StartServer();
                _dataService.ModbusTCP.StartTcpSlave(SlaveId);
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
            StatusText = StatusMessages.CONNECT_MSG_1 + Ip + StatusMessages.CONNECT_MSG_2 + Port + "\n";

            _dataService.ModbusIP = new ModbusIp(Ip, Port);
            try
            {
                _dataService.ModbusIP.StartClient();
                _dataService.ModbusIP.StartIpMaster();
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
                _dataService.ModbusTCP.StopTcpSlave(SlaveId);
                _dataService.ModbusTCP.StopServer();
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
                _dataService.ModbusIP.StopClient();
                _dataService.ModbusIP.StopIpMaster();
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
            switch (SelectedTabIndex)
            {
                case TAB_SERVER:
                    WriteHoldingFromServer();
                    break;
                case TAB_CLIENT:
                    WriteHoldingFromClient();
                    break;
                default:
                    break;
            }
        }

        public void WriteHoldingFromServer()
        {
            if (ModWriteHoldingAddr <= 0xFFFF)
            {
                _dataService.ModbusTCP.WriteTcpSlaveHoldingRegister(SlaveId, ModWriteHoldingAddr, ModWriteHoldingValue);
                UpdateTcpReadList(ModbusDataTypes.RD_HOLDINGREG);
                StatusText = StatusMessages.WRITE_HOLDING_MSG_1 + ModWriteHoldingAddr +
                    StatusMessages.WRITE_HOLDING_MSG_2 + ModWriteHoldingValue;
            }
            else
            {
                StatusText = StatusMessages.INVALID_ADDRESS;
            }
        }

        public void WriteHoldingFromClient()
        {
            try
            {
                if (ModWriteHoldingAddr <= 0xFFFF)
                {
                    _dataService.ModbusIP.WriteMasterHoldingRegister(
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
                _dataService.ModbusTCP.WriteTcpSlaveInputRegister(SlaveId, ModWriteInputAddr, ModWriteInputValue);
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

        private void TcpTimerTick(object send, EventArgs e)
        {
            InvokeDispatcher(() =>
            {
                UpdateTcpReadList(ModbusDataTypes.RD_INPUTREG);
                UpdateTcpReadList(ModbusDataTypes.RD_HOLDINGREG);
            });
        }

        private void UpdateTcpReadList(byte fc)
        {
            try
            {
                switch (fc)
                {
                    case ModbusDataTypes.RD_INPUTREG:
                        for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                        {
                            ushort address = (ushort)(idx + SavedModStartReadAddr);
                            ushort inputRegister = _dataService.ModbusTCP.ReadTcpSlaveInputRegister(SlaveId, address);

                            UpdateReadItem(ModReadInputList, idx, address, inputRegister);
                        }
                        break;
                    case ModbusDataTypes.RD_HOLDINGREG:
                        for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                        {
                            ushort address = (ushort)(idx + SavedModStartReadAddr);
                            ushort holdingRegister = _dataService.ModbusTCP.ReadTcpSlaveHoldingRegister(SlaveId, address);

                            UpdateReadItem(ModReadHoldingList, idx, address, holdingRegister);
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
            InvokeDispatcher(() =>
            {
                UpdateIpReadList(ModbusDataTypes.RD_INPUTREG);
                UpdateIpReadList(ModbusDataTypes.RD_HOLDINGREG);
            });
        }

        private void UpdateIpReadList(byte fc)
        {
            ushort[] data;

            try
            {
                switch (fc)
                {
                    case ModbusDataTypes.RD_INPUTREG:
                        data = _dataService.ModbusIP.ReadIpMasterInputRegisters(
                            SlaveId, SavedModStartReadAddr, SavedModReadAddrLength);
                        for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                        {
                            int address = idx + SavedModStartReadAddr;
                            UpdateReadItem(ModReadInputList, idx, (ushort)address, data[idx]);
                        }
                        break;
                    case ModbusDataTypes.RD_HOLDINGREG:
                        data = _dataService.ModbusIP.ReadIpMasterHoldingRegisters(
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

        private void UpdateReadItem(ObservableCollection<ModbusDataValue> collection, int index, ushort address, ushort register)
        {
            if (collection.Count() <= index)
            {
                collection.Add(new ModbusDataValue(address, register));
            }
            else if (collection[index].Address != address || collection[index].Register != register)
            {
                collection.RemoveAt(index);
                collection.Insert(index, new ModbusDataValue((ushort)address, register));
            }
        }

        private void InvokeDispatcher(Action action)
        {
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => action()));
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
                    Com = System.IO.Ports.SerialPort.GetPortNames().FirstOrDefault();
                    Baud = item.Baud;
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
            timer.Interval = TimeSpan.FromMilliseconds(1000);
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
                _dataService.ModbusTCP.Dispose();
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