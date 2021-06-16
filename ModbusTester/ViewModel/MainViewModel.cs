using System.Timers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ModbusCom;
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
        private const int TAB_SLAVE = 2;
        private const int TAB_MASTER = 3;

        private readonly IDataService _dataService;

        private Timer _timer = null;

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
        private bool _useHoldingRegister;
        private bool _useInputRegister;

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

        public bool UseHoldingRegister
        {
            get => _useHoldingRegister;
            set => Set(ref _useHoldingRegister, value);
        }

        public bool UseInputRegister
        {
            get => _useInputRegister;
            set => Set(ref _useInputRegister, value);
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
                    ConnectServer();
                    break;
                case TAB_CLIENT:
                    ConnectClient();
                    break;
                case TAB_SLAVE:
                    ConnectSlave();
                    break;
                case TAB_MASTER:
                    ConnectMaster();
                    break;
                default:
                    break;
            }
        }

        public void DisconnectCmdMethod()
        {
            switch (SelectedTabIndex)
            {
                case TAB_SERVER:
                    DisconnectServer();
                    break;
                case TAB_CLIENT:
                    DisconnectClient();
                    break;
                case TAB_SLAVE:
                    DisconnectSlave();
                    break;
                case TAB_MASTER:
                    DisconnectMaster();
                    break;
                default:
                    break;
            }
        }

        public void ConnectServer()
        {
            Connect(() =>
            {
                _dataService.ModbusTCP = new ModbusTcp(null, Port);
                _dataService.ModbusTCP.StartServer();
                _dataService.ModbusTCP.StartTcpSlave(SlaveId);
                IsServerConnected = true;
            });
        }

        public void ConnectClient()
        {
            Connect(() =>
            {
                _dataService.ModbusIP = new ModbusIp(Ip, Port);
                _dataService.ModbusIP.StartClient();
                _dataService.ModbusIP.StartIpMaster();
                IsClientConnected = true;
            });
        }

        public void ConnectSlave()
        {
            Connect(() =>
            {
                int baud = 9600;
                int.TryParse(Baud, out baud);
                _dataService.ModbusSlave = new ModbusSlave(Com, baud);
                _dataService.ModbusSlave.StartSlave();
                _dataService.ModbusSlave.StartSerialSlave(SlaveId);
                IsServerConnected = true;
            });
        }

        public void ConnectMaster()
        {
            Connect(() =>
            {
                int baud = 9600;
                int.TryParse(Baud, out baud);
                _dataService.ModbusMaster = new ModbusMaster(Com, baud);
                _dataService.ModbusMaster.StartMaster();
                _dataService.ModbusMaster.StartSerialMaster();
                IsClientConnected = true;
            });
        }

        public void DisconnectServer()
        {
            Disconnect(() =>
            {
                _dataService.ModbusTCP.StopTcpSlave(SlaveId);
                _dataService.ModbusTCP.StopServer();
                IsServerConnected = false;
            });
        }

        public void DisconnectClient()
        {
            Disconnect(() =>
            {
                _dataService.ModbusIP.StopIpMaster();
                _dataService.ModbusIP.StopClient();
                IsClientConnected = false;
            });
        }

        public void DisconnectSlave()
        {
            Disconnect(() =>
            {
                _dataService.ModbusSlave.StopSerialSlave(SlaveId);
                _dataService.ModbusSlave.StopSlave();
                IsServerConnected = false;
            });
        }

        public void DisconnectMaster()
        {
            Disconnect(() =>
            {
                _dataService.ModbusMaster.StopSerialMaster();
                _dataService.ModbusMaster.StopMaster();
                IsClientConnected = false;
            });
        }

        private void Connect(Action connectAction)
        {
            StatusText = StatusMessages.CONNECT_MSG_1 + Ip + "/" + Com +
                StatusMessages.CONNECT_MSG_2 + Port + "/" + Baud + "\n";

            try
            {
                connectAction();
                IsDisconnected = false;
                _timer.Start();
                StatusText += StatusMessages.SUCCESS;
            }
            catch (Exception e)
            {
                StatusText += StatusMessages.ERROR_HEADER + e.Message;
            }
        }

        public void Disconnect(Action disconnectAction)
        {
            try
            {
                _timer.Stop();
                disconnectAction();
                ModReadHoldingList.Clear();
                ModReadInputList.Clear();
                StatusText = StatusMessages.DISCONNECTED;
                IsDisconnected = true;
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
            if (!UseHoldingRegister) return;

            switch (SelectedTabIndex)
            {
                case TAB_SERVER:
                    WriteHoldingFromServer();
                    break;
                case TAB_CLIENT:
                    WriteHoldingFromClient();
                    break;
                case TAB_SLAVE:
                    WriteHoldingFromSlave();
                    break;
                case TAB_MASTER:
                    WriteHoldingFromMaster();
                    break;
                default:
                    break;
            }
        }

        public void WriteHoldingFromServer()
        {
            if (ModWriteHoldingAddr <= 0xFFFF)
            {
                _dataService.ModbusTCP.WriteTcpSlaveHoldingRegister(
                    SlaveId, ModWriteHoldingAddr, ModWriteHoldingValue);
                UpdateReadList(ModbusDataTypes.RD_HOLDINGREG);
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
                    _dataService.ModbusIP.WriteIpMasterHoldingRegister(
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

        public void WriteHoldingFromSlave()
        {
            if (ModWriteHoldingAddr <= 0xFFFF)
            {
                _dataService.ModbusSlave.WriteSerialSlaveHoldingRegister(
                    SlaveId, ModWriteHoldingAddr, ModWriteHoldingValue);
                UpdateReadList(ModbusDataTypes.RD_HOLDINGREG);
                StatusText = StatusMessages.WRITE_HOLDING_MSG_1 + ModWriteHoldingAddr +
                    StatusMessages.WRITE_HOLDING_MSG_2 + ModWriteHoldingValue;
            }
            else
            {
                StatusText = StatusMessages.INVALID_ADDRESS;
            }
        }

        public void WriteHoldingFromMaster()
        {
            try
            {
                if (ModWriteHoldingAddr <= 0xFFFF)
                {
                    _dataService.ModbusMaster.WriteSerialMasterHoldingRegister(
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
            if (!UseInputRegister) return;

            if (ModWriteInputAddr <= 0xFFFF)
            {
                if (SelectedTabIndex == TAB_SERVER)
                {
                    _dataService.ModbusTCP.WriteTcpSlaveInputRegister(SlaveId, ModWriteInputAddr, ModWriteInputValue);
                }
                else
                {
                    _dataService.ModbusSlave.WriteSerialSlaveInputRegister(SlaveId, ModWriteInputAddr, ModWriteInputValue);
                }

                UpdateReadList(ModbusDataTypes.RD_INPUTREG);

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

        private void ReadTimerTick(object sender, ElapsedEventArgs e)
        {
            if (UseInputRegister)
            {
                UpdateReadList(ModbusDataTypes.RD_INPUTREG);
            }

            if (UseHoldingRegister)
            {
                UpdateReadList(ModbusDataTypes.RD_HOLDINGREG);
            }

            (sender as Timer).Enabled = true;
        }

        private void UpdateReadList(byte fc)
        {
            try
            {
                Func<byte, ushort, ushort> reader = null;
                ushort[] data = null;
                ObservableCollection<ModbusDataValue> list = null;

                switch (SelectedTabIndex)
                {
                    case TAB_SERVER:
                        switch (fc)
                        {
                            case ModbusDataTypes.RD_INPUTREG:
                                list = ModReadInputList;
                                reader = _dataService.ModbusTCP.ReadTcpSlaveInputRegister;
                                break;
                            case ModbusDataTypes.RD_HOLDINGREG:
                                list = ModReadHoldingList;
                                reader = _dataService.ModbusTCP.ReadTcpSlaveHoldingRegister;
                                break;
                        }
                        break;
                    case TAB_SLAVE:
                        switch (fc)
                        {
                            case ModbusDataTypes.RD_INPUTREG:
                                list = ModReadInputList;
                                reader = _dataService.ModbusSlave.ReadSerialSlaveInputRegister;
                                break;
                            case ModbusDataTypes.RD_HOLDINGREG:
                                list = ModReadHoldingList;
                                reader = _dataService.ModbusSlave.ReadSerialSlaveHoldingRegister;
                                break;
                        }
                        break;
                    case TAB_CLIENT:
                        switch (fc)
                        {
                            case ModbusDataTypes.RD_INPUTREG:
                                list = ModReadInputList;
                                data = _dataService.ModbusIP.ReadIpMasterInputRegisters(
                                    SlaveId, SavedModStartReadAddr, SavedModReadAddrLength);
                                break;
                            case ModbusDataTypes.RD_HOLDINGREG:
                                list = ModReadHoldingList;
                                data = _dataService.ModbusIP.ReadIpMasterHoldingRegisters(
                                    SlaveId, SavedModStartReadAddr, SavedModReadAddrLength);
                                break;
                        }
                        break;
                    case TAB_MASTER:
                        switch (fc)
                        {
                            case ModbusDataTypes.RD_INPUTREG:
                                list = ModReadInputList;
                                data = _dataService.ModbusMaster.ReadSerialMasterInputRegisters(
                                    SlaveId, SavedModStartReadAddr, SavedModReadAddrLength);
                                break;
                            case ModbusDataTypes.RD_HOLDINGREG:
                                list = ModReadHoldingList;
                                data = _dataService.ModbusMaster.ReadSerialMasterHoldingRegisters(
                                    SlaveId, SavedModStartReadAddr, SavedModReadAddrLength);
                                break;
                        }
                        break;
                }

                for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                {
                    ushort address = (ushort)(idx + SavedModStartReadAddr);

                    if (reader != null)
                    {
                        UpdateReadItem(list, idx, address, reader(SlaveId, address));
                    }
                    else if (data != null)
                    {
                        UpdateReadItem(list, idx, address, data[idx]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                StatusText = "Read Error: " + e.Message;
            }
        }

        private void UpdateReadItem(ObservableCollection<ModbusDataValue> collection, int index, ushort address, ushort register)
        {
            if (collection.Count() <= index)
            {
                InvokeDispatcher(() => collection.Add(new ModbusDataValue(address, register)));

            }
            else if (collection[index].Address != address || collection[index].Register != register)
            {
                InvokeDispatcher(() =>
                {
                    collection.RemoveAt(index);
                    collection.Insert(index, new ModbusDataValue((ushort)address, register));
                });
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

            UseHoldingRegister = true;
            UseInputRegister = true;

            // Timer
            _timer = new Timer(950)
            {
                AutoReset = false
            };
            _timer.Elapsed += ReadTimerTick;
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
                try
                {
                    _dataService.ModbusTCP?.Dispose();
                    _dataService.ModbusIP?.Dispose();
                    _dataService.ModbusMaster?.Dispose();
                    _dataService.ModbusSlave?.Dispose();
                }
                catch { }
            }
        }

        public override void Cleanup()
        {
            // Clean up if needed
            Dispose();

            base.Cleanup();
        }
    }
}