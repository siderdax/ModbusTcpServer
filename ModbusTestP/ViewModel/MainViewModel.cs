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

        private readonly IDataService _dataService;
        private ModbusTcp modbusTcp;

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
            StatusText = StatusMessages.CONNECT_MSG_1 + Ip +
                StatusMessages.CONNECT_MSG_2 + Port + "\n";

            modbusTcp = new ModbusTcp(Ip, Port);
            try
            {
                modbusTcp.StartServer();
                modbusTcp.StartTcpSlave(SlaveId);
                IsDisconnected = false;
                timer.Start();
            }
            catch (Exception e)
            {
                StatusText += StatusMessages.ERROR_HEADER + e.Message;
            }

            StatusText += StatusMessages.SUCCESS;
        }

        public void DisconnectCmdMethod()
        {
            try
            {
                timer.Stop();

                modbusTcp.StopTcpSlave(SlaveId);
                modbusTcp.StopServer();

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
            ModStartReadAddr = ModStartReadAddr > 0 ? ModStartReadAddr : (ushort) 1;
            ModReadAddrLength = ModReadAddrLength > 0 ? ModReadAddrLength : (ushort) 1;
            if (ModStartReadAddr + ModReadAddrLength > 0x10000)
            {
                ModReadAddrLength = (ushort) (0x10000 - ModStartReadAddr);
            }
            SavedModReadAddrLength = ModReadAddrLength;
            SavedModStartReadAddr = ModStartReadAddr;

            ModReadInputList.Clear();
            ModReadHoldingList.Clear();

            StatusText = StatusMessages.SETTINGS_SAVED;
        }

        public void WriteHoldingCmdMethod()
        {
            if (ModWriteHoldingAddr <= 0xFFFF)
            {
                modbusTcp.WriteTcpSlaveHoldingRegister(SlaveId, ModWriteHoldingAddr, ModWriteHoldingValue);
                UpdateReadList(ModbusDataTypes.RD_HOLDINGREG);
                StatusText = StatusMessages.WRITE_HOLDING_MSG_1 + ModWriteHoldingAddr +
                    StatusMessages.WRITE_HOLDING_MSG_2 + ModWriteHoldingValue;
            }
            else
            {
                StatusText = StatusMessages.INVALID_ADDRESS;
            }
        }

        public void WriteInputCmdMethod()
        {
            if (ModWriteInputAddr <= 0xFFFF)
            {
                modbusTcp.WriteTcpSlaveInputRegister(SlaveId, ModWriteInputAddr, ModWriteInputValue);
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

        /// <summary>
        /// Modbus server Event
        /// </summary>
        public void HoldingRegisterChanged(int register, int numberOfRegisters)
        {
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                UpdateReadList(ModbusDataTypes.RD_HOLDINGREG);
            else
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => UpdateReadList(ModbusDataTypes.RD_HOLDINGREG)));

            StatusText = StatusMessages.HOLDING_CHANGED_MSG_1 + register +
                StatusMessages.HOLDING_CHANGED_MSG_2 + numberOfRegisters;
        }

        /// Other Functions

        private void UpdateReadList(byte fc)
        {
            switch (fc)
            {
                case ModbusDataTypes.RD_INPUTREG:
                    for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                    {
                        int address = idx + SavedModStartReadAddr;
                        ushort inputRegister = modbusTcp.ReadTcpSlaveInputRegister(
                            SlaveId, (ushort) address);

                        UpdateReadItem(ModReadInputList, idx, (ushort) address, inputRegister);
                    }
                    break;
                case ModbusDataTypes.RD_HOLDINGREG:
                    for (int idx = 0; idx < SavedModReadAddrLength; idx++)
                    {
                        int address = idx + SavedModStartReadAddr;
                        ushort holdingRegister = modbusTcp.ReadTcpSlaveHoldingRegister(
                            SlaveId, (ushort) address);

                        UpdateReadItem(ModReadHoldingList, idx, (ushort) address, holdingRegister);
                    }
                    break;
                default:
                    break;
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
                    collection.Insert(index, new ModbusDataValue((ushort) address, register));
                }
            }

        }

        /// <summary>
        /// Timer
        /// </summary>
        private DispatcherTimer timer = null;
        private void TimerTick(object send, EventArgs e)
        {
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                UpdateReadList(ModbusDataTypes.RD_INPUTREG);
                UpdateReadList(ModbusDataTypes.RD_HOLDINGREG);
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => UpdateReadList(ModbusDataTypes.RD_INPUTREG)));
                System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => UpdateReadList(ModbusDataTypes.RD_HOLDINGREG)));
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

            // Timer
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            timer = dispatcherTimer;
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += new EventHandler(TimerTick);
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