using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using NLog;
using System.Text;

namespace SerialDeviceLib
{
    public class SerialDeviceInput
    {
        internal static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Private variables
        /// </summary>
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;

        private CancellationTokenSource ReadCancellationTokenSource;
        #region Private Fields
        private string _portName;
        private uint _baudrate = 115200;
        private ushort _dataBits = 8;
        private SerialStopBitCount _stopbits= SerialStopBitCount.One;
        private SerialParity _parity=SerialParity.None;

        private object accessLock = new object();
        /// <summary>
        /// 状态
        /// </summary>
        private string Status { get; set; }
        /// <summary>
        /// 串口获取状态,
        /// true:有可用串口
        /// false:无可用串口
        /// </summary>
        private bool IsEnabled;//
        private bool IsConnect=false;//
        #endregion

        #region Public Events
        /// <summary>
        /// SerialDeviceInput里边运行的当前状态
        /// </summary>
        public string SerialDeviceInputSatus { get => Status; }

        /// <summary>
        /// 获取可用串口状态
        /// </summary>
        public bool SerialPortIsEnabled { get => IsEnabled; }

        #endregion
        #region Public Events
        /// <summary>
        /// ComPort received event.
        /// </summary>
        public delegate void ComPortWasFoundEventHandler(object sender, ComPortWasFoundEventArgs args);
        /// <summary>
        /// Occurs when message received.
        /// </summary>
        public event ComPortWasFoundEventHandler ComPortReceived;
        /// <summary>
        /// Connected state changed event.
        /// </summary>
        public delegate void ConnectionStatusChangedEventHandler(object sender, ConnectionStatusChangedEventArgs args);
        /// <summary>
        /// Occurs when connected state changed.
        /// </summary>
        public event ConnectionStatusChangedEventHandler ConnectionStatusChanged;

        /// <summary>
        /// Message received event.
        /// </summary>
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs args);
        /// <summary>
        /// Occurs when message received.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;

        #endregion
        #region Public Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portname"></param>
        /// <param name="baudrate"></param>
        /// <param name="parity"></param>
        /// <param name="DataBits"></param>
        /// <param name="stopbits"></param>
        public void SetPort(string portname, uint baudrate = 115200, SerialParity parity = SerialParity.None, ushort DataBits=8, SerialStopBitCount stopbits = SerialStopBitCount.One)
        {
            if (portname != null)
            {
                this._portName = portname;
                this._baudrate = baudrate;
                this._stopbits = stopbits;
                this._parity = parity;
                Status = "串口设置成功";
            }
            else
            {
                Status = "串口号为空";
            }
        }
        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                Status = "Reading task was cancelled, closing device and cleaning up";
                CloseDevice();
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }
        //转换数据  
        private string LoadData(uint bytesRead)
        {
            StringBuilder str_builder = new StringBuilder();
            //转换缓冲区数据为16进制  
            while (dataReaderObject.UnconsumedBufferLength > 0)
            {
                str_builder.Append(dataReaderObject.ReadByte().ToString("x2"));
            }
            return str_builder.ToString().ToUpper();
        }


        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            //Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            //// If task cancellation was requested, comply
            //cancellationToken.ThrowIfCancellationRequested();

            //// Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            //dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            //using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            //{
            //    // Create a task object to wait for data on the serialPort.InputStream
            //    loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

            //    // Launch the task and wait
            //    UInt32 bytesRead = await loadAsyncTask;
            //    if (bytesRead > 0)
            //    {
            //        rcvdText.Text = dataReaderObject.ReadString(bytesRead);
            //        status.Text = "bytes read successfully!";
            //    }
            //}
            Task<UInt32> loadAsyncTask;
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            //读取数据  
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask();
            uint bytesRead = await loadAsyncTask;
            //判断获取数据长度  
            if (bytesRead > 0)
            {
                //转换十六进制数据  
                string res = LoadData(bytesRead);
                OnMessageReceived(new MessageReceivedEventArgs(res));
               // Status = "Recived Data:"+ res;
            }
        }
        public async void Connect()
        {

            try
            {
                serialPort = await SerialDevice.FromIdAsync(this._portName);
                if (serialPort == null) return;
                //串口已连接
                IsConnect = true;
                OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs(true));
                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(200);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(200);
                serialPort.BaudRate =this._baudrate;
                serialPort.Parity = this._parity;
                serialPort.StopBits =this._stopbits;
                serialPort.DataBits =this._dataBits;
                serialPort.Handshake = SerialHandshake.None;

                // Display configured settings
                Status = "串口: ";
                Status += serialPort.BaudRate + "-";
                Status += serialPort.DataBits + "-";
                Status += serialPort.Parity.ToString() + "-";
                Status += serialPort.StopBits;

                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();
                Listen();
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }
        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        public void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
                IsConnect = false;
                OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs(false));
                Status = "串口关闭";
            }
            serialPort = null;
        }
        /// <summary>
        /// 检测到可用串口以对象集合返回，dis[i]选择,dis.Count查看总数,serialPort = await SerialDevice.FromIdAsync(dis[i].Id);
        /// </summary>
        /// <returns></returns>
        public async void GetAvailablePorts_Dis()
        {
            DeviceInformationCollection dis = null;
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                dis = await DeviceInformation.FindAllAsync(aqs);

                Status = "检测到可用串口";
                this.IsEnabled = true;
                ComPortWasFound(new ComPortWasFoundEventArgs(dis));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private char[] HexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f' };
        private bool CharInArray(char aChar, char[] charArray)
        {
            return (Array.Exists<char>(charArray, delegate (char a) { return a == aChar; }));
        }
        /// <summary>

        /// 十六进制字符串转换字节数组

        /// </summary>

        /// <param name="s"></param>

        /// <returns></returns>

        public byte[] HexStringToByteArray(string s)
        {
            // s = s.Replace(" ", "");
            if (s.Length % 2 != 0)
            {
                s = "0" + s;
            }
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char aChar in s)
            {
                if (CharInArray(aChar, HexDigits))
                    sb.Append(aChar);
            }
            s = sb.ToString();
            int bufferlength;
            if ((s.Length % 2) == 1)
                bufferlength = s.Length / 2 + 1;
            else bufferlength = s.Length / 2;
            byte[] buffer = new byte[bufferlength];
            for (int i = 0; i < bufferlength - 1; i++)
                buffer[i] = (byte)Convert.ToByte(s.Substring(2 * i, 2), 16);
            if (bufferlength > 0)
                buffer[bufferlength - 1] = (byte)Convert.ToByte(s.Substring(2 * (bufferlength - 1), (s.Length % 2 == 1 ? 1 : 2)), 16);
            return buffer;
        }

        /// <summary>

        /// 字节数组转换十六进制字符串

        /// </summary>

        /// <param name="data"></param>

        /// <returns></returns>

        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }

        /// <summary>
        ///十六进制字符串转换为ASCII
        /// </summary>
        /// <param name="hexstring">一条十六进制字符串</param>
        /// <returns>返回一条ASCII码</returns>
        public string HexStringToAscii(string hexstring)
        {
            byte[] ss = HexStringToByteArray(hexstring);
            string AA = null;
            AA= new ASCIIEncoding().GetString(ss,0,ss.Length);
 
            return AA;
        }


        /**/
        /// <summary>
        /// 16进制字符串转换为二进制数组
        /// </summary>
        /// <param name="hexstring">用空格切割字符串</param>
        /// <returns>返回一个二进制字符串</returns>
        private byte[] HexStringToBinary(string hexstring)
        {

            string[] tmpary = hexstring.Trim().Split(' ');
            byte[] buff = new byte[tmpary.Length];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = Convert.ToByte(tmpary[i], 16);
            }
            return buff;
        }
        
        /// <summary>
        /// 文本方式发送字符串
        /// </summary>
        public async void SendMessage_StringAsText(string sendStr)
        {
            try
            {
                if (serialPort != null && IsConnect== true)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteStringAsync(sendStr);
                    
                }
                else
                {
                    Status ="串口未打开，发送数据失败";
                }
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        /// <summary>
        /// 输入16进制字符串按照16进制字节数组发送
        /// </summary>
        /// <param name="sendStr">16进制字符串</param>
        public async void SendMessage_HexStringAsHexBytes(string sendStr)
        {
            try
            {
                if (serialPort != null && IsConnect == true)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);
                  
                    //Launch the WriteAsync task to perform the write
                    await WriteBytesAsync(HexStringToByteArray(sendStr));

                }
                else
                {
                    Status = "串口未打开，发送数据失败";
                }
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        /// <summary>
        /// byte[]数组转为对应字符串发送
        /// </summary>
        /// <param name="sendStr"></param>
        public async void SendMessage_BytesAsHexString(byte[] sendStr)
        {
            try
            {
                if (serialPort != null && IsConnect == true)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteStringAsync(ByteArrayToHexString(sendStr));

                }
                else
                {
                    Status = "串口未打开，发送数据失败";
                }
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        /// <summary>
        /// byte[]数组直接发送
        /// </summary>
        /// <param name="sendBytes"></param>
        public async void SendMessage_BytesAsBytes(byte[] sendBytes)
        {
            try
            {
                if (serialPort != null && IsConnect == true)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteBytesAsync(sendBytes);

                }
                else
                {
                    Status = "串口未打开，发送数据失败";
                }
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        /// <summary>
        /// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        /// </summary>
        /// <returns></returns>
        private async Task WriteStringAsync(string sendDataStr)
        {
            Task<UInt32> storeAsyncTask;

            if (sendDataStr.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                dataWriteObject.WriteString(sendDataStr);

                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    Status = "Send String：" + sendDataStr;
                    Status += "bytes written successfully!";
                }
                sendDataStr = "";
            }
            else
            {
                Status = "Enter the text you want to write and then click on 'WRITE'";
            }
        }
        /// <summary>
        /// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        /// </summary>
        /// <returns></returns>
        private async Task WriteBytesAsync(byte[] sendDataByte)
        {
            Task<UInt32> storeAsyncTask;

            if (sendDataByte.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                dataWriteObject.WriteBytes(sendDataByte);

                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    Status = "Send String：" + BitConverter.ToString(sendDataByte);
                    Status += "bytes written successfully!";
                }
                sendDataByte = null;
            }
            else
            {
                Status = "Enter the text you want to write and then click on 'WRITE'";
            }
        }
        #region Events Raising

        /// <summary>
        /// Raises the connected state changed event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnConnectionStatusChanged(ConnectionStatusChangedEventArgs args)
        {
            logger.Debug(args.ConnectStatus);
            if (ConnectionStatusChanged != null)
                ConnectionStatusChanged(this, args);
        }

        /// <summary>
        /// Raises the message received event.
        /// </summary>
        /// <param name="args">Arguments.</param>
        protected virtual void OnMessageReceived(MessageReceivedEventArgs args)
        {
            logger.Debug(args.Data.ToString());
            if (MessageReceived != null)
                MessageReceived(this, args);
        }
        /// <summary>
        /// Raises the Comport can use event.
        /// </summary>
        /// <param name="args">ObservableCollection<DeviceInformation></param>
        protected virtual void ComPortWasFound(ComPortWasFoundEventArgs args)
        {
            logger.Debug(args.DeviceInformationCollection[0].Id.ToString());
            if (ComPortReceived != null)
                ComPortReceived(this, args);
        }
        #endregion

    }
}
/// <summary>
/// Connected state changed event arguments.
/// </summary>
public class ConnectionStatusChangedEventArgs
{
    /// <summary>
    /// The connected state.
    /// </summary>
    public readonly bool ConnectStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialDeviceLib.ConnectionStatusChangedEventArgs"/> class.
    /// </summary>
    /// <param name="state">State of the connection (true = connected, false = not connected).</param>
    public ConnectionStatusChangedEventArgs(bool state)
    {
        ConnectStatus = state;
    }
}

/// <summary>
/// Message received event arguments.
/// </summary>
public class MessageReceivedEventArgs
{
    /// <summary>
    /// The data.
    /// </summary>
    public readonly string Data;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialDeviceLib.MessageReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="data">Data：返回数据</param>
    public MessageReceivedEventArgs(string data)
    {
        Data = data;
    }
}
/// <summary>
/// 发现可用串口事件
/// </summary>
public class ComPortWasFoundEventArgs
{
    /// <summary>
    /// DeviceInformationCollection[i].ID串口号
    /// DeviceInformationCollection.Count
    /// </summary>
    public readonly DeviceInformationCollection DeviceInformationCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialDeviceLib.ComPortReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="DeviceInformationCollection">DeviceInformationCollection.ID为串口号</param>
    public ComPortWasFoundEventArgs(DeviceInformationCollection data)
    {
        DeviceInformationCollection = data;
    }
}

#endregion